using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

using insp.Utility.Command;
using insp.Utility.IO;
using insp.Utility.Collections;
using insp.Utility.Collections.Tree;
using insp.Utility.Collections.Time;

using insp.Security.Data;
using insp.Security.Data.kline;
using insp.Security.Data.Indicator;

using insp.Utility.Bean;
using insp.Utility.Common;

using insp.Security.Strategy;
using insp.Security.Strategy.Alpha;

using Security.Alpha.Application.State;
using insp.Utility.Date;
using insp.Utility.Text;

namespace Security.Alpha.Application
{
    /// <summary>
    /// 消息回调
    /// </summary>
    /// <param name="state"></param>
    /// <param name="msg"></param>
    public delegate void HandleMessage(LogRecord state,int beginorend,int error=0,String msg="",String detailMsg="");

    /// <summary>
    /// 执行调度
    /// </summary>
    public class Dispatcher
    {
        #region 常量
        /// <summary>
        /// 事件开始标记
        /// </summary>
        public const int BEGIN = 1;
        /// <summary>
        /// 事件过程标记
        /// </summary>
        public const int PROGRESS = 2;
        /// <summary>
        /// 事件结束标记
        /// </summary>
        public const int END = 3;
        /// <summary>
        /// 参数文件
        /// </summary>
        public const String PARAM_FILE = "alpha.propreties";
        #endregion

        #region 状态
        /// <summary>
        /// 当前日期
        /// </summary>
        public readonly DateTime tradeDate;
        /// <summary>
        /// 状态记录
        /// </summary>
        public readonly LogFile StateRecords;
        /// <summary>
        /// 消息处理回调，由主界面实现
        /// </summary>
        public HandleMessage msgHandler;
        /// <summary>
        /// 行情仓库
        /// </summary>
        private IndicatorRepository repository;
        /// <summary>
        /// 参数文件
        /// </summary>
        private insp.Utility.Bean.Properties props;

        /// <summary>
        /// 显示和记录任务开始
        /// </summary>
        /// <param name="msg"></param>
        private void showBeginMessage(String msg)
        {
            if (this.msgHandler != null)
                msgHandler(StateRecords.CurrentState, BEGIN, 0, msg);
        }
        /// <summary>
        /// 显示和记录任务结束
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="error"></param>
        /// <param name="detailMsg"></param>
        private void showResultMessage(String msg="",int error=0,String detailMsg="")
        {
            StateRecords.CurrentState.recordEnd(msg, error, detailMsg);
            StateRecords.Save();

            if (this.msgHandler != null)
                msgHandler(StateRecords.CurrentState, END, 0, msg,detailMsg);
        }

        private void showProgressMessage(String data)
        {
            if (StateRecords.CurrentState == null) return;
            StateRecords.CurrentState.data = data;
            if (this.msgHandler != null)
                msgHandler(StateRecords.CurrentState, PROGRESS);

        }
        
        #endregion

        #region 初始化
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="date"></param>
        /// <param name="msgHandler"></param>
        public Dispatcher(DateTime date,HandleMessage msgHandler)
        {
            tradeDate = date;
            StateRecords = new LogFile(date);
            this.msgHandler = msgHandler;
            init();
        }

        private void init()
        {
            props = insp.Utility.Bean.Properties.Load(FileUtils.GetDirectory() + "PARAM_FILE", Encoding.UTF8);
            //注册转换器
            ConvertUtils.RegisteConvertor<String, TradeInfo>(ConvertUtils.strToObject<TradeInfo>);
            ConvertUtils.RegisteConvertor<TradeInfo, String>(ConvertUtils.objectToStr);
            ConvertUtils.RegisteConvertor<String, TradeDirection>(ConvertUtils.strtoenum<TradeDirection>);
            ConvertUtils.RegisteConvertor<TradeDirection, String>(ConvertUtils.enumtostr<TradeDirection>);
            ConvertUtils.RegisteConvertor<String, TradeIntent>(ConvertUtils.strtoenum<TradeIntent>);
            ConvertUtils.RegisteConvertor<TradeIntent, String>(ConvertUtils.enumtostr<TradeIntent>);

        }
        #endregion

        #region 总调度执行
        /// <summary>
        /// 调度执行
        /// </summary>
        public void Execute()
        {
            LogRecord state = StateRecords.CurrentState;
            if (state == null)
                StateRecords.CurrentState = new LogRecord(DateTime.Now, LogRecord.EVENT_DOWNLOADING, "", "");
            state = StateRecords.CurrentState;
            //下载行情
            if (state.eventName == LogRecord.EVENT_DOWNLOADING)
            {
                if (!doDownloadDayLine())
                    return;

                StateRecords.CurrentState = new LogRecord(DateTime.Now, LogRecord.EVENT_REPOSITORY, "", "");
            }
            //新行情入库
            if (state.eventName == LogRecord.EVENT_REPOSITORY)
            {
                if (!doMergeToRepository())
                    return;
                StateRecords.CurrentState = new LogRecord(DateTime.Now, LogRecord.EVENT_GENERATE, "", "");
            }
            //生成新指标
            if(state.eventName == LogRecord.EVENT_GENERATE)
            {
                if (!doGenerateIndicator())
                    return;
                StateRecords.CurrentState = new LogRecord(DateTime.Now, LogRecord.EVENT_SELL, "", "");
            }

            //判断卖出
            if(state.eventName == LogRecord.EVENT_SELL)
            {
                if (!doSell())
                    return;
                StateRecords.CurrentState = new LogRecord(DateTime.Now, LogRecord.EVENT_BUY, "", "");
            }

            //判断买入
            if(state.eventName == LogRecord.EVENT_BUY)
            {
                if (!doBuy())
                    return;
                StateRecords.CurrentState = new LogRecord(DateTime.Now, LogRecord.EVENT_ENTER, "", "");
            }

            //等待管理员确认
            if(state.eventName == LogRecord.EVENT_ENTER)
            {
                if (!doMaualEnter())
                    return;
                StateRecords.CurrentState = new LogRecord(DateTime.Now, LogRecord.EVENT_WAIT_TRADE, "", "");
            }

            //等待下单时间到达
            if (state.eventName == LogRecord.EVENT_WAIT_TRADE)
            {
                if (!doWaitEntrustTimeArrive())
                    return;
                StateRecords.CurrentState = new LogRecord(DateTime.Now, LogRecord.EVENT_DO_TRADE, "", "");
            }

            //执行下单委托
            if (state.eventName == LogRecord.EVENT_DO_TRADE)
            {
                doEntrust();                
            }

        }
        #endregion

        #region 行情下载
        /// <summary>
        /// 下载日线
        /// </summary>
        public bool doDownloadDayLine()
        {
            try
            {
                showBeginMessage("开始下载日线...");
                //读取下载日线命令文件
                String filename = FileUtils.GetDirectory() + "download.cmd";
                String[] lines = File.ReadAllLines(filename);

                //分析命令
                CommandContext context = new CommandContext();
                CommandInterpreter interpreter = new CommandInterpreter(filename, context);
                interpreter.Parse();

                //执行命令
                interpreter.Execute();

                showResultMessage("下载日线完成");
                

                return true;
            }
            catch(Exception e)
            {
                showResultMessage("下载日线失败,任务终止执行", -1, e.Message);
                return false;
            }

        }
        #endregion

        #region 新K线入库
        /// <summary>
        /// 导入数据
        /// </summary>
        public bool doMergeToRepository()
        {
            showBeginMessage("开始合并数据...");
            if (repository == null)
            {
                repository = new IndicatorRepository(FileUtils.GetDirectory(props.Get<String>("repository")));
                repository.Initilization();
            }
            String datapath = FileUtils.GetDirectory(props.Get<String>("datapath"));

            DirectoryInfo dInfo = new DirectoryInfo(datapath);
            FileInfo[] fInfos = dInfo.GetFiles("*.scv");
            if(fInfos == null || fInfos.Length<=0)
            {
                showResultMessage("没有需要导入的新文件", 1);
                return false;
            }
            try
            {
                foreach (FileInfo fInfo in fInfos)
                {
                    String code = fInfo.Name.Substring(3, 6);
                    TimeSerialsDataSet ds = repository[code];
                    KLine kline = ds.DayKLine;
                    if (ds == null) continue;
                    showProgressMessage(code);                    
                    CSVFile csvFile = new CSVFile();
                    csvFile.Load(fInfo.FullName, Encoding.UTF8, false, ",");
                    List<String> lines = csvFile.Lines;
                    for (int i = lines.Count - 1; i >= 0; i--)
                    {
                        if (lines[i] == null || lines[i].Trim() == "") continue;
                        String[] ss = lines[i].Split(',');
                        if (ss == null || ss.Length < 7) continue;

                        DateTime d = DateUtils.Parse(ss[0]);
                        KLineItem item = kline[d];
                        if (item != null) break;

                        double[] v = new double[6];
                        for (int j = 0; j < v.Length; j++)
                            v[j] = double.Parse(ss[j + 1]);
                        item = new KLineItem();
                        item.Date = d;
                        item.SetValue<double>("OPEN", v[0]);
                        item.SetValue<double>("HIGH", v[1]);
                        item.SetValue<double>("LOW", v[2]);
                        item.SetValue<double>("CLOSE", v[3]);
                        item.SetValue<double>("VOLUME", v[4]);
                        item.SetValue<double>("TURNOVER", v[5]);
                        kline.Add(item);
                    }

                    ds.Save("kline", TimeUnit.day);                    
                }
                showResultMessage("");
                return true;
            }
            catch(Exception e)
            {
                showResultMessage("导入失败", -1,e.Message);
                return false;
            }


        }
        #endregion

        #region 生成指标数据
        /// <summary>
        /// 生成指标数据
        /// </summary>
        /// <returns></returns>
        public bool doGenerateIndicator()
        {
            showBeginMessage("开始生成指标...");
            if (repository == null)
            {
                repository = new IndicatorRepository(FileUtils.GetDirectory(props.Get<String>("repository")));
                repository.Initilization();
            }
            try
            {
                List<String> codes = repository.Securities.Codes;
                foreach (String code in codes)
                {
                    TimeSerialsDataSet ds = repository[code];
                    if (ds == null) continue;
                    showProgressMessage(code);
                    KLine kline = ds.DayKLine;
                    TradingLine tradeLine = ds.DayTradeLine;
                    ds.Create("kline", TimeUnit.week);
                    ds.Create("kline", TimeUnit.month);
                    ds.CubeCreate();
                    ds.CubeCreate(TimeUnit.week);
                    ds.CubeCreate(TimeUnit.month);
                    ds.FundTrendCreate(TimeUnit.day);
                    ds.FundTrendCreate(TimeUnit.week);
                    ds.FundTrendCreate(TimeUnit.month);
                    ds.FundTrendCrossCreate(TimeUnit.day);
                    ds.FundTrendCrossCreate(TimeUnit.week);
                    ds.FundTrendCrossCreate(TimeUnit.month);
                }
                showResultMessage("");
                return true;
            }
            catch(Exception e)
            {
                showResultMessage("生成指标失败", -1, e.Message);
                return false;
            }
        }
        #endregion

        #region 判断卖出条件
        /// <summary>
        /// 策略上下文
        /// </summary>
        public StrategyContext context;
        /// <summary>
        /// 持仓记录
        /// </summary>
        public readonly List<HoldRecord> holdRecords = new List<HoldRecord>();
        /// <summary>
        /// 卖出委托
        /// </summary>
        public readonly List<TradeInfo> sellEntrust = new List<TradeInfo>();
        /// <summary>
        /// 取得持仓信息
        /// </summary>
        public void doGetHolds()
        {
            String holdFilename = FileUtils.GetDirectory("records") + "holds.csv";
            holdRecords.Clear();
            if(File.Exists(holdFilename))
            {
                new List<String>(File.ReadAllLines(holdFilename)).ForEach(
                    x => { HoldRecord r = HoldRecord.Parse(x); if (r != null) holdRecords.Add(r); }
                    );
            }
        }

        private StrategyContext createStrategyContext(bool forced = false)
        {
            if (!forced && context != null)
                return context;
            String codefile = props.Get<String>("codefile");
            context = new StrategyContext();
            context.Put("codefile", codefile);
            context.Put("repository", repository);
            return context;
        }
        /// <summary>
        /// 卖出操作
        /// </summary>
        /// <returns></returns>
        public bool doSell()
        {
            showBeginMessage("选择卖出股票...");
            try
            {
                //取得持仓
                doGetHolds();
                if (holdRecords.Count <= 0)
                    return true;
                //创建策略上下文
                createStrategyContext();
                //取得策略
                String strategyname = props.Get<String>("strategy");                                
                KeyValuePair<IStrategyMeta,insp.Utility.Bean.Properties> kv = context.GetStrateMetaAndParam(strategyname);
                if (kv.Key == null || kv.Value == null)
                    throw new Exception("缺少有效策略");
                StrategyInstance instance = (StrategyInstance)kv.Key.CreateInstance("1", kv.Value, "");

                //取得卖出算法类
                String sellername = kv.Value.Get<String>("seller");
                Seller seller = (Seller)instance.Seller;
                if (instance.Seller == null)
                    throw new Exception("缺少有效的卖出策略:" + sellername);

                //对持仓逐个判断是否要卖出
                sellEntrust.Clear();
                String tradeFilename = FileUtils.GetDirectory("records") + "trades.csv";
                foreach (HoldRecord hold in holdRecords)
                {
                    insp.Utility.Bean.Properties sellParams = seller.PDC.CreateProperties(hold.parameters);
                    TradeInfo tradeInfo = seller.DoSell(hold, this.tradeDate, sellParams,context);
                    if (tradeInfo == null) continue;
                    File.AppendAllLines(tradeFilename, new string[] { tradeInfo.ToString() });
                    sellEntrust.Add(tradeInfo);
                }
                showResultMessage("");
                return true;
            }catch(Exception e)
            {
                showResultMessage("卖出操作异常", -1, e.Message);                
                return false;
            }
            

        }


        #endregion

        #region 判断买入
        /// <summary>
        /// 买入委托数据
        /// </summary>
        public readonly List<TradeInfo> buyEntrust = new List<TradeInfo>();
        /// <summary>
        /// 买入操作
        /// </summary>
        /// <returns></returns>
        public bool doBuy()
        {
            showBeginMessage("选择买入股票...");
            try
            {
                //创建策略上下文
                createStrategyContext();
                //取得策略
                String strategyname = props.Get<String>("strategy");
                KeyValuePair<IStrategyMeta, insp.Utility.Bean.Properties> kv = context.GetStrateMetaAndParam(strategyname);
                if (kv.Key == null || kv.Value == null)
                    throw new Exception("缺少有效策略");
                StrategyInstance instance = (StrategyInstance)kv.Key.CreateInstance("1", kv.Value, "");

                //取得买入算法类
                String buyername = kv.Value.Get<String>("buyer");
                Buyer buyer = (Buyer)instance.Buyer;
                if (buyer == null)
                    throw new Exception("缺少有效的卖出策略:" + buyername);

                List<TradeInfo> tradeInfos = buyer.DoBuy(kv.Value, this.tradeDate, context);


                if (tradeInfos != null && tradeInfos.Count >= 0)
                {
                    String tradeFilename = FileUtils.GetDirectory("records") + "trades.csv";
                    buyEntrust.AddRange(tradeInfos);
                    File.AppendAllLines(tradeFilename, buyEntrust.ConvertAll(x=>x.ToText()).ToArray());
                }

                
                return true;
            }catch(Exception e)
            {
                showResultMessage("买入操作异常", -1, e.Message);
                return false;
            }
        }
        #endregion

        #region 委托操作
        public const int ADMIN_WAIT = 0;
        public const int ADMIN_OK = 1;
        public const int ADMIN_CANCEL = 2;
        public int adminEnter = 0;
        /// <summary>
        /// 确认操作
        /// </summary>
        /// <returns></returns>
        public bool doMaualEnter()
        {
            showBeginMessage("等待确认...");
            String adminenter = props.Get<String>("adminenter");
            if (adminenter == null || adminenter.Trim() == "")
                adminenter = "true";
            if (adminenter.ToLower() == "false")
            {
                showResultMessage("无需确认");
                return true;
            }
            adminEnter = 0;
            while (true)
            {
                if (adminEnter != 0) break;
                System.Windows.Forms.Application.DoEvents();
            }
            if(adminEnter == ADMIN_OK)
            {
                showResultMessage("管理员已确认");
                return true;
            }else
            {
                showResultMessage("管理员已取消",1);
                return true;
            }

                
        }

        public bool waitingEntrust = false;
        public bool doWaitEntrustTimeArrive()
        {
            String time = props.Get<String>("entrustTime");
            if (time == null || time.Trim() == "")
            {
                return true;
            }
            showBeginMessage("等待委托时间:"+time);
            waitingEntrust = true;
            while (waitingEntrust)
            {
                try
                {
                    System.Threading.Thread.Sleep(60000);
                }
                catch (ThreadAbortException e)
                {
                    return false;
                }
                catch (Exception) { }

                if (DateTime.Now.Hour >= 12) continue;

                String now = DateTime.Now.ToString("h:m:s");
                
                if(now.CompareTo(time)>0)
                {
                    showResultMessage();
                    return true;
                }
                
            }
            return true;
        }
        #endregion

        #region 委托
        public bool doEntrust()
        {
            try
            {
                showBeginMessage("开始委托交易");
                //读取下载日线命令文件
                String filename = FileUtils.GetDirectory() + "entrust.cmd";
                

                //准备环境
                CommandContext context = new CommandContext();
                List<TradeInfo> entrust = new List<TradeInfo>();
                if (sellEntrust.Count > 0)
                    entrust.AddRange(sellEntrust);
                if (buyEntrust.Count > 0)
                    entrust.AddRange(buyEntrust);
                context.PutExternalValue("entrust", entrust);

                //执行命令
                CommandInterpreter interpreter = new CommandInterpreter(filename, context);
                interpreter.Parse();                
                interpreter.Execute();

                showResultMessage("委托完成");


                return true;
            }
            catch (Exception e)
            {
                showResultMessage("委托失败,任务终止执行", -1, e.Message);
                return false;
            }
        }
        #endregion
    }
}
