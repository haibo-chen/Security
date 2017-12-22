using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using insp.Utility.Bean;
using insp.Utility.Collections;


using insp.Utility.Collections.Tree;
using insp.Utility.Collections.Time;

using insp.Security.Data;
using insp.Security.Data.kline;
using insp.Security.Data.Indicator;

using insp.Utility.IO;
using insp.Utility.Log;
using System.Threading;
using insp.Utility.Common;
using insp.Security.Data.Indicator.Fund;
using insp.Utility.Date;

namespace insp.Security.Strategy
{
    public abstract class StrategyInstance : IStrategyInstance
    {
        #region 属性
        /// <summary>
        /// ID
        /// </summary>
        protected String id = "";
        /// <summary>
        /// ID
        /// </summary>
        public string ID { get { return id; } }

        /// <summary>
        /// 配置参数
        /// </summary>
        protected Properties props = null;
        /// <summary>
        /// 配置参数
        /// </summary>
        public Properties Properties { get { return props; } }

        /// <summary>
        /// 版本
        /// </summary>
        public abstract Version Version { get; }
        /// <summary>
        /// 策略元
        /// </summary>
        public IStrategyMeta Meta { get; set; }


        protected StrategyInstanceStatus status = StrategyInstanceStatus.Creating;

        /// <summary>
        /// 当前状态 
        /// </summary>
        public StrategyInstanceStatus Status { get { return status; } }


        #endregion


        #region 初始化
        static StrategyInstance()
        {
            //注册转换器
            ConvertUtils.RegisteConvertor<String, TradeInfo>(ConvertUtils.strToObject<TradeInfo>);
            ConvertUtils.RegisteConvertor<TradeInfo, String>(ConvertUtils.objectToStr);
            ConvertUtils.RegisteConvertor<String, TradeDirection>(ConvertUtils.strtoenum<TradeDirection>);
            ConvertUtils.RegisteConvertor<TradeDirection, String>(ConvertUtils.enumtostr<TradeDirection>);
            ConvertUtils.RegisteConvertor<String, TradeIntent>(ConvertUtils.strtoenum<TradeIntent>);
            ConvertUtils.RegisteConvertor<TradeIntent, String>(ConvertUtils.enumtostr<TradeIntent>);
            ConvertUtils.RegisteConvertor<String, GetInMode>((x,format,props) => GetInMode.Parse(x));            
        }
            


        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Initilization(Properties props = null)
        {
            if (props != null)
                this.props = props;

        }
        #endregion

        #region 参数管理
        /// <summary>
        /// 所有参数
        /// </summary>
        protected Dictionary<PropertyDescriptor, Object> parameters;
        /// <summary>
        /// 所有参数
        /// </summary>
        public Dictionary<PropertyDescriptor, Object> Parameters
        {
            get
            {
                if (parameters != null)
                    return parameters;
                parameters = new Dictionary<PropertyDescriptor, object>();
                foreach(PropertyDescriptor pd in Meta.Parameters)
                {
                    parameters.Add(pd, this.props.Get<Object>(pd.Name));
                }
                
                return parameters;
            }
        }
        /// <summary>
        /// 查询指定参数
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public K GetParameter<K>(String name)
        {
            PropertyDescriptor pd = this.Meta.Parameters.Get(name);
            if (pd == null)
                throw new Exception(name + "无效:属性名没有定义");
            if (!Parameters.ContainsKey(pd))
                throw new Exception(name + "无效:属性名没有注册");
            return (K)(Object)Parameters[pd];
        }

        #endregion

        #region 显示
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            List<PropertyDescriptor> pds = Meta.Parameters;
            foreach (PropertyDescriptor pd in pds)
            {
                if (str.ToString() != "")
                    str.Append(",");
                str.Append(pd.Caption + "=" + this.props.Get<String>(pd.Name));
            }
            return Meta.ToString() + ":" + ID + "[" + str.ToString() + "]";

        }

        #endregion

        #region 回测

        /// <summary>
        /// 日志对象
        /// </summary>
        protected log4net.ILog log = null;
        /// <summary>
        /// 回测参数
        /// </summary>
        protected BacktestParameter backtestParam;

        /// <summary>
        /// 执行回测
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public virtual TotalStat DoTest(IStrategyContext context, Properties props)
        {
            //取得回测参数
            backtestParam = new BacktestParameter(props);
            log = log4net.LogManager.GetLogger(backtestParam.serialno);

            log.Info("");
            log.Info("回测策略实例:" + this.ToString());
            log.Info("回测数据路径=" + backtestParam.datapath);
            log.Info("准备回测：回测编号=" + backtestParam.serialno + ",初始资金=" + backtestParam.initfunds.ToString("F2") + ",日期=" + backtestParam.beginDate.ToString("yyyyMMdd") + "-" + backtestParam.endDate.ToString("yyyyMMdd"));

            List<String> codes = new List<string>();
            System.IO.File.ReadAllLines(FileUtils.GetDirectory() + backtestParam.codefilename)
                .ToList().ForEach(x => codes.Add(x.Split(',')[1]));
            log.Info("加载代码" + codes.Count.ToString());

            List<TradeBout> bouts = doTestByCodes(codes);
            return doTestByDate(bouts);

        }

        protected abstract List<TradeBout> doTestByCodes(List<String> codes);
        /// <summary>
        /// 执行回测
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public virtual TotalStat doTestByDate(List<TradeBout> bouts)
        {
            double marketValueMin = backtestParam.InitFund;//日最低市值
            double marketValueMax = backtestParam.InitFund;//日最高市值
            double lastmarketValueMax = backtestParam.InitFund;//上一个日最高市值
            DateTime lastmarketValueMaxDate = backtestParam.beginDate;
            double curFund = backtestParam.InitFund;       //当前资金

            TotalStat stat = new TotalStat();
            List<DateDetailRecord> records = new List<DateDetailRecord>();//日详细记录            
            List<TradeBout> holdTrades = new List<TradeBout>();//日持仓回合
            List<int> holdDays = new List<int>();//持仓日期
            List<String> codes = new List<string>();//交易的股票代码
            List<int> buyCounts = new List<int>();//每天买入的回合数

            IndicatorRepository repository = (IndicatorRepository)backtestParam.Get<Object>("repository");

            //遍历每一天
            for (DateTime d = backtestParam.beginDate; d <= backtestParam.endDate; d = d.AddDays(1))
            {
                //跳过非工作日
                //if (!CalendarUtils.IsWorkDay(d))
                //     continue;


                //生成空的当日记录
                DateDetailRecord record = new DateDetailRecord();
                record.date = d;
                //找到当日的买入回合、卖出回合
                bouts.ForEach(y =>
                {
                    if (y.BuyInfo.TradeDate.Date == d.Date) record.buyBouts.Add(y);
                    else if (y.SellInfo.TradeDate.Date == d.Date) record.sellBouts.Add(y);
                });
                
                //当日没有发生买卖操作，也没有持仓，跳过
                if (record.buyBouts.Count <= 0 && record.sellBouts.Count <= 0 && holdTrades.Count <= 0)
                    continue;



                //将buyTrades按照优先规则排序,待实现

                //计算当日买入的花销，如果超过了资金允许买入的量，则删除一部分
                record.willBuyCount = record.buyBouts.Count;
                for (int i = 0; i < record.buyBouts.Count; i++)
                {
                    if (record.buyBouts[i].BuyInfo.TradeCost > curFund)//资金不够
                    {
                        bouts.Remove(record.buyBouts[i]);
                        record.buyBouts.RemoveAt(i--);
                    }
                    else
                    {
                        curFund -= record.buyBouts[i].BuyInfo.TradeCost; //买入
                        holdTrades.Add(record.buyBouts[i]);              //买入后变成持仓
                    }
                }
                record.buyCount = record.buyBouts.Count;
                if (stat.MaxTradeCountPerDay < record.buyBouts.Count)
                    stat.MaxTradeCountPerDay = record.buyBouts.Count;
                buyCounts.Add(record.buyBouts.Count);


                

                //卖出收入放回资金
                for (int i = 0; i < record.sellBouts.Count; i++)
                {
                    if (!codes.Contains(record.sellBouts[i].Code))//记录交易的股票
                        codes.Add(record.sellBouts[i].Code);
                    stat.BoutNum += 1; //回合数加1
                    if (record.sellBouts[i].Win)//胜数加1
                        stat.WinNum += 1;
                    holdDays.Add(record.sellBouts[i].PositionDays);//记录持仓日期

                    curFund += record.sellBouts[i].SellInfo.TradeCost;//回收资金
                    holdTrades.Remove(record.sellBouts[i]);            //从持仓中拿掉
                }
                record.SellCount = record.sellBouts.Count;

                //计算市值
                record.holdCount = holdTrades.Count;
                if (holdTrades.Count <= 0)//如果没有持仓，市值就是资金量
                {
                    marketValueMax = marketValueMin = curFund;
                    records.Add(record);
                }
                else//如果有持仓，则计算市值=资金量+持仓当日市值
                {
                    double min = 0, max = 0;
                    foreach (TradeBout info in holdTrades)
                    {
                        TimeSerialsDataSet ds = repository[info.Code];
                        KLine kline = ds.DayKLine;
                        KLineItem klineitem = kline.GetNearest(d, true, -1);
                        if (klineitem == null)//有一个回合找不到当日K线数据，则当日市值不再计算
                        {
                            min = max = 0;
                            this.log.Warn("日期" + d.ToString("yyyyMMdd") + "中有回合缺少当日和历史K线：" + info.Code);
                            break;
                        }
                        min += info.BuyInfo.Amount * klineitem.LOW;
                        max += info.BuyInfo.Amount * klineitem.HIGH;
                        record.holdBouts.Add(info.Code + "," + info.BuyInfo.Amount + "," + info.BuyInfo.TradePrice.ToString("F2") + "," + klineitem.CLOSE);
                    }
                    if (min != 0)
                    {
                        marketValueMin = curFund + min;
                    }
                    if (max != 0)
                        marketValueMax = curFund + max;
                    if (min != 0 && max != 0)
                    {
                        records.Add(record);
                    }


                }
                //记录资金和市值数据
                record.curFund = curFund;
                record.marketValueMin = marketValueMin;
                record.marketValueMax = marketValueMax;
                if (marketValueMin < backtestParam.InitFund)
                    record.retracement = (backtestParam.InitFund - marketValueMin) / backtestParam.InitFund;
                if (stat.MaxInitRetracementRate < record.retracement)
                {
                    stat.MaxInitRetracementRate = record.retracement;
                    stat.MaxInitRetracementDate = d;
                }
                if (marketValueMax > lastmarketValueMax)
                {
                    lastmarketValueMax = marketValueMax;
                    lastmarketValueMaxDate = d;
                    record.retracement = 0;
                }
                else
                {
                    record.retracement = (lastmarketValueMax - marketValueMax) / lastmarketValueMax;
                    stat.MaxRetracementRate = record.retracement;
                    stat.MaxRetracementDate = d;
                }

            }

            //清除没有卖出的回合
            for (int i = 0; i < holdTrades.Count; i++)
            {
                curFund += holdTrades[i].BuyInfo.TradeCost;
                bouts.Remove(holdTrades[i]);
            }
            holdTrades.Clear();
            marketValueMin = marketValueMax = curFund;
            if (records.Count > 0)
            {
                DateDetailRecord extends = new DateDetailRecord();
                extends.date = records[records.Count - 1].date.AddDays(1);
                extends.curFund = curFund;
                extends.marketValueMax = marketValueMax;
                extends.marketValueMin = marketValueMin;
                records.Add(extends);
            }

            //结果统计
            stat.Records = records;

            stat.AverageTradeCountPerDay = buyCounts.Count<=0?0:buyCounts.Average();
            stat.AvgHoldDays = holdDays.Count<=0?0:(int)holdDays.Average();
            stat.Count = codes.Count;
            stat.MaxHoldDays = holdDays.Count <= 0 ? 0 : holdDays.Max();
            stat.TotalFund = curFund;
            stat.TotalProfilt = (curFund - backtestParam.InitFund) / backtestParam.InitFund;
            stat.WinRate = stat.WinNum * 1.0 / stat.BoutNum;

            return stat;

        }
        /// <summary>
        /// 删除未完成的测试回合
        /// </summary>
        /// <param name="bouts"></param>
        protected void RemoveUnCompeletedBouts(List<TradeBout> bouts)
        {
            if (bouts == null) return;
            for (int i = 0; i < bouts.Count; i++)
            {
                if (bouts[i].Completed)
                    continue;
                bouts.RemoveAt(i--);
            }
        }
        #endregion

        #region 执行
        /// <summary>
        /// 实际执行
        /// </summary>
        public abstract void Run(Properties context);

        /// <summary>
        /// 事件触发动作
        /// </summary>
        /// <param name="context"></param>
        /// <param name="args"></param>
        public abstract void DoAction(IStrategyContext context, EventArgs args);
        #endregion
    }
}
