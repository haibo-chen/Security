using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using insp.Security.Strategy;
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

namespace insp.Security.Strategy.Alpha
{
    
    /// <summary>
    /// Alpha策略实例
    /// </summary>
    public class AlphaStrategy401Instance : StrategyInstance, IIndicatorContext
    {
        
        #region 属性
        

        /// <summary>
        /// 版本
        /// </summary>
        public override Version Version { get { return new Version(4, 0, 1); } }

        #endregion

        #region 初始化
        

        /// <summary>
        /// 构造方法
        /// </summary>
        internal AlphaStrategy401Instance(Properties props) { id = "1"; this.props = props; }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="id"></param>
        internal AlphaStrategy401Instance(String id, Properties props) { this.id = id; this.props = props; }

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Initilization(Properties props = null)
        {
            if (props != null)
                this.props = props;
            //目前没有对参数进行校验
            p_mainforcelow = this.props.Get<int>("mainforcelow");
            p_mainforcerough = this.props.Get<int>("mainforcerough");
            p_maxprofilt = this.props.Get<double>("maxprofilt");
            p_maxholddays = this.props.Get<int>("maxholddays");
            p_stoploss = this.props.Get<double>("stoploss");
            p_fundpergetin = GetInMode.Parse(this.props.Get<String>("fundpergetin"));
            p_buypointdays = this.props.Get<int>("buypointdays");
            p_maxbuynum = this.props.Get<int>("maxbuynum");
            p_grail = GrailParameter.Parse(this.props.Get<String>("grail"));
            status = StrategyInstanceStatus.Idle;
        }

        #endregion

        #region 执行参数

        /// <summary>
        /// 主力线低位
        /// </summary>
        protected int p_mainforcelow;
        /// <summary>
        /// 主力线地位模糊值
        /// </summary>
        protected int p_mainforcerough;
        /// <summary>
        /// 最大收益
        /// </summary>
        protected double p_maxprofilt;
        /// <summary>
        /// 最大持仓天数
        /// </summary>
        protected int p_maxholddays;
        
        /// <summary>
        /// 止损线
        /// </summary>
        protected double p_stoploss;        
        /// <summary>
        /// 每次建仓的资金，它和最大持仓股票数只有一个有效，缺省是“每次建仓的资金”
        /// </summary>
        protected GetInMode p_fundpergetin;
        /// <summary>
        /// 买点附近天数
        /// </summary>
        protected int p_buypointdays;
        /// <summary>
        /// 单只股票加仓次数
        /// </summary>
        protected int p_maxbuynum;
        /// <summary>
        /// 大盘参数
        /// </summary>
        protected GrailParameter p_grail;
        /// <summary>
        /// 用于比较主力线位置的比较方法
        /// </summary>
        private Func<ITimeSeriesItem<List<double>>, double, double> comparasion = (x, y) => (double)(x.Value[0] - y);

        /// <summary>
        /// 用于比较K线的最高点位置的方法
        /// </summary>
        private Func<KLineItem, double, double> comparasionK = (x, y) => (double)(x.HIGH - y);

        
        public override string ToString()
        {
            return Meta.ToString() + ":" + ID + "[主力线低位=" + p_mainforcelow.ToString() +
                                               ",低位阈值=" + p_mainforcerough.ToString() +
                                               ",平仓收益=" + p_maxprofilt.ToString("F2") +
                                               ",最大持股天数=" + p_maxholddays +
                                               ",建仓资金=" + p_fundpergetin.ToString()+
                                               ",止损=" + p_stoploss.ToString("F2") +                                               
                                               ",买点附近=" + p_buypointdays.ToString()+"天"+
                                               ",加仓最大次数=" + p_maxbuynum.ToString() +
                                               ",大盘控制="+p_grail.Enable.ToString()+
                                               "]";
     
        }
        #endregion

        #region 回测

        #region 回测参数
        /// <summary>
        /// 日志对象
        /// </summary>
        protected log4net.ILog log = null;
        /// <summary>
        /// 回测参数
        /// </summary>
        protected BacktestParameter backtestParam;

        
        /// <summary>
        /// 回测数据集
        /// </summary>
        protected ConcurrentBag<StrategyDataSet> dataset = new ConcurrentBag<StrategyDataSet>();
        /// <summary>
        /// 上次回测代码
        /// </summary>
        protected String backtestlastpos = "";
        
        #endregion


        #region 回测入口

        /// <summary>
        /// 执行回测
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public TotalStat DoTest(IStrategyContext context, Properties props)
        {
            //初始化回测环境
            
            dataset = new ConcurrentBag<StrategyDataSet>();

            //取得回测参数
            backtestParam = new BacktestParameter(props);
            log = log4net.LogManager.GetLogger(backtestParam.serialno);

            log.Info("");
            log.Info("回测策略实例:" + this.ToString());
            log.Info("回测数据路径=" + backtestParam.datapath);            
            log.Info("准备回测：回测编号=" + backtestParam.serialno + ",初始资金=" + backtestParam.initfunds.ToString("F2") + ",日期=" + backtestParam.beginDate.ToString("yyyyMMdd") + "-" + backtestParam.endDate.ToString("yyyyMMdd"));


            //执行回测
            doTestByCodes(null);
            TotalStat stat =doTestByDate(context, props);

            //统计回测结果            
            stat.Summary(log);

            //写结果文件
            recordBacktest(stat);

            return stat;
        }
      
        #endregion

        #region 按照日期循环进行回测统计        
        /// <summary>
        /// 执行回测
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public TotalStat doTestByDate(IStrategyContext context, Properties props)
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

            p_grail.Init(backtestParam.datapath,backtestParam.resultpath); //初始化大盘指数数据
            char[] drails = new char[3] { 'U', 'U', 'U' }; //三个大盘的买卖点状态，U表示未知
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
                this.dataset.ToList().ForEach(x => x.tradeRecords.Bouts.ForEach(y => {
                    if (y.BuyInfo.TradeDate.Date == d.Date) record.buyBouts.Add(y);
                    else if (y.SellInfo.TradeDate.Date == d.Date) record.sellBouts.Add(y);                    
                }));
                

                //当日没有发生买卖操作，也没有持仓，跳过
                if (record.buyBouts.Count <= 0 && record.sellBouts.Count <= 0 && holdTrades.Count <= 0)
                    continue;



                //将buyTrades按照优先规则排序,待实现

                //计算当日买入的花销，如果超过了资金允许买入的量，则删除一部分
                record.willBuyCount = record.buyBouts.Count;
                for (int i=0;i< record.buyBouts.Count;i++)
                {
                    if(!p_grail.CanBuy(d, record.buyBouts[i].Code))//大盘限制买
                    {
                        this.dataset.FirstOrDefault(x => x.code == record.buyBouts[i].Code).tradeRecords.Bouts.Remove(record.buyBouts[i]);
                        record.buyBouts.RemoveAt(i--);
                    }
                    else if(record.buyBouts[i].BuyInfo.TradeCost > curFund)//资金不够
                    {
                        this.dataset.FirstOrDefault(x => x.code == record.buyBouts[i].Code).tradeRecords.Bouts.Remove(record.buyBouts[i]);
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

                
                //对于持仓数据，将大盘发出S点的转移到待卖出中去
                for (int i=0;i< holdTrades.Count;i++)
                {
                    if (!p_grail.MustSell(d, holdTrades[i].Code))
                        continue;
                    holdTrades[i].SellInfo.Reason = "大盘卖点";
                    record.sellBouts.Add(holdTrades[i]);
                    holdTrades.RemoveAt(i--);
                }

                //卖出收入放回资金
                for (int i=0;i< record.sellBouts.Count;i++)
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
                if (holdTrades.Count<=0)//如果没有持仓，市值就是资金量
                {
                    marketValueMax = marketValueMin = curFund;
                    records.Add(record);
                }
                else//如果有持仓，则计算市值=资金量+持仓当日市值
                {                
                    double min = 0, max = 0;
                    foreach (TradeBout info in holdTrades)
                    {
                        KLine kline = GetKline(info.Code);
                        KLineItem klineitem = kline.GetNearest(d,true,-1);
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
                    if(min != 0)
                    {
                        marketValueMin = curFund + min;                        
                    }                        
                    if(max != 0)
                        marketValueMax = curFund + max;
                    if(min != 0 && max != 0)
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
                if(stat.MaxInitRetracementRate < record.retracement)
                {
                    stat.MaxInitRetracementRate = record.retracement;
                    stat.MaxInitRetracementDate = d;
                }
                if(marketValueMax > lastmarketValueMax)
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
            for(int i=0;i< holdTrades.Count;i++)
            {
                curFund += holdTrades[i].BuyInfo.TradeCost;
                this.dataset.FirstOrDefault(x => x.code == holdTrades[i].Code).tradeRecords.Bouts.Remove(holdTrades[i]);
            }
            holdTrades.Clear();
            marketValueMin = marketValueMax = curFund;
            if(records.Count>0)
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

            stat.AverageTradeCountPerDay = buyCounts.Average();
            stat.AvgHoldDays = (int)holdDays.Average();
            stat.Count = codes.Count;
            stat.MaxHoldDays = holdDays.Max();
            stat.TotalFund = curFund;
            stat.TotalProfilt = (curFund - backtestParam.InitFund) / backtestParam.InitFund;
            stat.WinRate = stat.WinNum*1.0 / stat.BoutNum;

            return stat;

        }
        #endregion

        #region 按照股票代码遍历

        /// <summary>
        /// 执行回测
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        protected override List<TradeBout> doTestByCodes(List<String> codes)
        {
            
            //取得回测代码集
            if(codes==null|| codes.Count<=0) codes = loadCodes(backtestParam.datapath, backtestParam.codefilename);

            //读取上次记录
            backtestlastpos = FileUtils.ReadText(backtestParam.StateFileName);

            //并行执行   
            if (backtestParam.runparallel.ToLower() == "true")
                Parallel.ForEach<String>(codes, doTestByCode);
            else
                codes.ForEach(x => doTestByCode(x));

            //没有完成的回合需要去除
            removeUncompleteBout();

            return null;
        }


        /// <summary>
        /// 执行某个代码的回测
        /// </summary>
        /// <param name="code"></param>
        private void doTestByCode(String code)
        {            
            //创建数据集
            StrategyDataSet ds = StrategyDataSet.CreateOrLoad(code, backtestParam.datapath, backtestParam.resultpath,true, backtestParam.serialno);
            if (ds == null)
                return;
            
            //判断上次执行状态保存,如果已经回测过，则跳过
            if (backtestlastpos != "" && backtestParam.runparallel.ToLower() != "true")
            {
                if (backtestlastpos != code)
                    return;
                backtestlastpos = "";
                return;
            }

            //若交易记录不空，则不用再次回测
            if (ds.tradeRecords != null && ds.tradeRecords.Count > 0)
                return;

            //K线数据无效则跳过
            if (ds.klineDay==null || ds.klineDay.Count <= 0)
                return;
            //资金数据无效则跳过
            if (ds.fundDay == null || ds.fundDay.Count <= 0)
                return;
            //买卖线数据无效则跳过
            if (ds.tradeLineDay == null ||
               ds.tradeLineDay.buyLine == null || ds.tradeLineDay.buyLine.Count <= 0 ||
               ds.tradeLineDay.sellLine == null || ds.tradeLineDay.sellLine.Count <= 0 ||
               ds.tradeLineDay.buysellPoints == null || ds.tradeLineDay.buysellPoints.Count <= 0)
                return;


            //执行回测
            for (DateTime d = backtestParam.beginDate; d <= backtestParam.endDate; d = d.AddDays(1))
            {
                //if (!CalendarUtils.IsWorkDay(d))
                //    continue;

                KLineItem dayLineItem = ds.klineDay[d];
                ITimeSeriesItem<List<double>> funds = ds.fundDay[d];
                if (funds == null) continue;

                List<TradeBout> sellBouts = doTestSell(d, ds, dayLineItem);
                TradeBout buyBout = doTestBuy(d, ds, dayLineItem, funds);
                if (buyBout != null)
                    d = buyBout.BuyInfo.TradeDate;
            }

            if (ds.tradeRecords.Count <= 0)
                return;
            
            dataset.Add(ds);
            log.Info(code + ":" + ds.tradeRecords.ToString());
            //ds.SaveTradeRecords(backtestParam.serialno);    
                

            //释放不用的内存
            ds.Dispose();
            //记录回测执行位置 
            if(backtestParam.runparallel.ToLower() != "true")
                System.IO.File.WriteAllText(backtestParam.StateFileName, ds.code);
            
            GC.Collect();
            GC.WaitForFullGCComplete();
        }

        

        
        #endregion

        

        #region 回测基本函数
        /// <summary>
        /// 回测买入
        /// </summary>
        /// <param name="d"></param>
        /// <param name="ds"></param>
        /// <param name="dayLineItem"></param>
        /// <param name="funds"></param>
        /// <returns></returns>
        private TradeBout doTestBuy(DateTime d, StrategyDataSet ds, KLineItem dayLineItem = null, ITimeSeriesItem<List<double>> funds = null)
        {
            if (dayLineItem == null)
                dayLineItem = ds.klineDay[d];
            if (funds == null)
                funds = ds.fundDay[d];

            //该股票有持仓的跳过
            if (p_maxbuynum > 0 && (ds.tradeRecords.Count - ds.tradeRecords.CountCompleted) >= p_maxbuynum)
                return null;
            //日期d已经出现在已经处理过的低位段了
            if (ds.fundLowRanges.FirstOrDefault(x => x.IsIn(d)) != null)
                return null;

            //主力线不在低位跳过
            if (funds.Value[0] > p_mainforcelow)
                return null;

            
            TradeBout result = null;
            //寻找资金主力线突破位置 
            DateTime tDate;
            ITimeSeriesItem<List<double>> boundFundItem = ds.fundDay.FindBoundayLineValue(funds.Date, p_mainforcelow, p_mainforcerough, comparasion,out tDate);
            if (boundFundItem == null)
                return null;
            ds.fundLowRanges.Add(new Range<DateTime>(d, boundFundItem.Date));
            String reason = "主力线=" + boundFundItem.Value[0];

            d = boundFundItem.Date;
            funds = boundFundItem;
            dayLineItem = ds.klineDay[d];

            //寻找对应买卖点
            ITimeSeriesItem<char> rt = null;
            if (p_buypointdays > 0)
            {
                DateTime td1 = d - new TimeSpan(p_buypointdays, 0, 0, 0, 0);
                DateTime td2 = d + new TimeSpan(p_buypointdays, 0, 0, 0, 0);
                if (ds.tradeLineDay.buysellPoints != null)
                {
                    TimeSeries<ITimeSeriesItem<char>> ts = ds.tradeLineDay.buysellPoints[td1, td2];
                    if (ts != null)
                        rt = ts.FirstOrDefault(x => x.Value == 'B');
                }
            }

            //满足以上条件的买入
            if (p_buypointdays <= 0 || rt != null)
            {
                double price = dayLineItem.CLOSE;
                double fund = p_fundpergetin.Value;// price * p_maxholdnum;

                int amount = (int)(fund / price);
                TradeBout newBout = new TradeBout(ds.code);
                newBout.RecordTrade(1, d, TradeDirection.Buy, price, amount, backtestParam.volumecommission, 0, reason);
                ds.tradeRecords.Bouts.Add(newBout);
                return newBout;
            }
            return null;
        }
        /// <summary>
        /// 数据ds中持仓的是否需要在日期d卖出
        /// </summary>
        /// <param name="d"></param>
        /// <param name="ds"></param>
        /// <returns></returns>
        private List<TradeBout> doTestSell(DateTime d, StrategyDataSet ds, KLineItem dayLineItem = null)
        {            
            List<TradeBout> results = new List<TradeBout>();

            if (dayLineItem == null)
                dayLineItem = ds.klineDay[d];

            //遍历每一个持仓 
            for (int i = 0; i < ds.tradeRecords.Bouts.Count; i++)
            {
                TradeBout bout = ds.tradeRecords.Bouts[i];
                if (bout.Completed) continue;//跳过已完成的
                if (bout.BuyInfo.TradeDate >= d)//当天买入的跳过
                    continue;
                double diff = dayLineItem.HIGH - bout.BuyInfo.TradePrice;
                double percent = diff / bout.BuyInfo.TradePrice;
                if (percent >= p_maxprofilt) //盈利超过预定
                {
                    double price = bout.BuyInfo.TradePrice * (1+ p_maxprofilt);
                    int amount = bout.BuyInfo.Amount;
                    bout.RecordTrade(2, d, TradeDirection.Sell, price, amount, backtestParam.volumecommission, backtestParam.stampduty,"盈利>="+ p_maxprofilt.ToString("F2"));
                    results.Add(bout);
                }
                else if (p_maxholddays != 0 && CalendarUtils.WorkDayCount(bout.BuyInfo.TradeDate,d) >= p_maxholddays) //持仓超过n天
                {
                    bool selled = false;
                    DateTime sellDate = DateUtils.InitDate;
                    DateTime beginTestDate = d,endTestDate  = d;

                    //尝试15日内以不亏损的方式卖出
                    int index = ds.klineDay.IndexOf(d);
                    for(int k = 0; k < 15; k++)
                    {
                        if (ds.klineDay.Count <= index + k + 1)
                            break;
                        KLineItem item = ds.klineDay[index + k + 1];
                        if(item != null && item.HIGH >= bout.BuyInfo.TradePrice)
                        {
                            int tamount = bout.BuyInfo.Amount;
                            bout.RecordTrade(2, item.Date, TradeDirection.Sell, bout.BuyInfo.TradePrice, tamount, backtestParam.volumecommission, backtestParam.stampduty, "持仓超过" + p_maxholddays.ToString() + "后第"+(k+1).ToString()+"日卖出");
                            results.Add(bout);
                            selled = true;
                            break;
                        }
                    }
                    if (selled) continue;


                    //分MAX_NUM次，每次降低预期利润一半，看daylength天内是否能达到
                    /*
                    int daylength = 5, MAX_NUM = 3;
                    for (int k=0;k< MAX_NUM; k++)
                    {
                        double tprice = bout.BuyInfo.TradePrice + bout.BuyInfo.TradePrice * p_maxprofilt * 1.0 / (Math.Pow(2, k + 1));
                        KLineItem tKLineItem = ds.klineDay.FindBoundayLineValue(beginTestDate, tprice, 0, comparasionK, out endTestDate, daylength);
                        if(tKLineItem != null)
                        {                            
                            int tamount = bout.BuyInfo.Amount;
                            bout.RecordTrade(2, tKLineItem.Date, TradeDirection.Sell, tprice, tamount, backtestParam.volumecommission, backtestParam.stampduty, "持仓超过" + p_maxholddays.ToString() + "利润降低"+ ((int)Math.Pow(2, k + 1)).ToString()+"卖出");
                            results.Add(bout);
                            selled = true;
                            break;
                        }
                        beginTestDate = endTestDate;
                        if (endTestDate.Date >= this.backtestParam.endDate.Date)
                            break;
                    }
                    if (selled) continue;
                    */

                    //再看5天，只要不亏损就卖出
                    /*
                    index = ds.klineDay.IndexOf(endTestDate);
                    if(index >= 0)
                    {
                        for (int k = index + 1; k <= index + 5; k++)
                        {
                            if (k >= ds.klineDay.Count) break;
                            KLineItem tItem = ds.klineDay[k];
                            if (tItem == null) break;
                            if (tItem.HIGH() >= bout.BuyInfo.TradePrice)
                            {
                                double tprice = bout.BuyInfo.TradePrice;
                                int tamount = bout.BuyInfo.Amount;
                                bout.RecordTrade(2, tItem.Date, TradeDirection.Sell, tprice, tamount, backtestParam.volumecommission, backtestParam.stampduty, "持仓超过" + p_maxholddays.ToString() + "不亏损卖出");
                                results.Add(bout);
                                selled = true;
                                break;
                            }
                        }
                    }
                    
                    if (selled) continue;
                    */

                    double price = dayLineItem.CLOSE;
                    int amount = bout.BuyInfo.Amount;
                    bout.RecordTrade(2, d, TradeDirection.Sell, price, amount, backtestParam.volumecommission, backtestParam.stampduty,"持仓超过"+ p_maxholddays.ToString());
                    results.Add(bout);
                }
                else if (p_stoploss > 0 && -1 * percent > p_stoploss)//达到止损线
                {
                    double price = dayLineItem.CLOSE;
                    int amount = bout.BuyInfo.Amount;
                    bout.RecordTrade(2, d, TradeDirection.Sell, price, amount, backtestParam.volumecommission, backtestParam.stampduty,"到达止损线"+ p_stoploss.ToString("F2"));
                    results.Add(bout);
                }

            }
            return results;
        }
        /// <summary>
        /// 去掉所有未完成回合 
        /// </summary>
        private void removeUncompleteBout()
        {
            foreach (StrategyDataSet ds in dataset)
            {
                for (int i = 0; i < ds.tradeRecords.Bouts.Count; i++)
                {
                    if (!ds.tradeRecords.Bouts[i].Completed)
                    {
                        //curfund += ds.tradeRecords.Bouts[i].BuyInfo.TradeCost;
                        ds.tradeRecords.Bouts.RemoveAt(i--);
                    }
                }
            }
        }
        
        /// <summary>
        /// 读取历史交易记录
        /// </summary>
        private void loadTradeRecords()
        {
            //读取记录
            List<String> codes = this.loadCodes(backtestParam.datapath, backtestParam.codefilename);
            Parallel.ForEach(codes, code => 
                {
                    StrategyDataSet ds = StrategyDataSet.CreateOrLoad(code, backtestParam.datapath, backtestParam.resultpath, true, backtestParam.serialno);
                    if (ds != null)
                        this.dataset.Add(ds);
                });            
        }
        /// <summary>
        /// 加载代码
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private List<String> loadCodes(String path,String filename)
        {
            
            if (filename == null || filename == "")
                filename = "stocks.txt";

            String fullfilename = path + filename;// "stocks.txt";
            if (!File.Exists(fullfilename))
                fullfilename = FileUtils.GetDirectory() + filename;
            List<String> strs = new List<string>();
            strs.AddRange(System.IO.File.ReadAllLines(filename).Select(x => x));
            strs = strs.ConvertAll(x => x.Split(',')[1].Trim());
            return strs;
        }
        
        /// <summary>
        /// 写回测结果
        /// </summary>
        private void recordBacktest(TotalStat stat)
        {
            #region 写入回测统计结果
            StringBuilder str = new StringBuilder();
            ////批号
            str.Append(backtestParam.serialno + ",");
            ////策略参数
            List<String> paramNames = Meta.GetParameterNames();
            for (int i = 0; i < paramNames.Count; i++)
            {
                str.Append(this.GetParameterValue<String>(paramNames[i]) + ",");
            }
            ////回测结果 
            str.Append(stat.Count.ToString() + ",");
            str.Append(stat.BoutNum.ToString() + ",");
            str.Append(stat.WinRate.ToString("F2") + ",");
            str.Append(stat.TotalProfilt.ToString("F2") + ",");
            str.Append(stat.TotalFund.ToString("F2") + ",");
            str.Append(stat.HoldDays + ",");
            str.Append(stat.InitRetracement + ",");
            str.Append(stat.TradeCountPerDay);
            str.Append(System.Environment.NewLine);
            
            //写文件
            System.IO.File.WriteAllText(backtestParam.ResultFileName, str.ToString());
            #endregion

            #region 写回测日期记录
            stat.WriteRecord(backtestParam.DateRecordFileName, backtestParam.DateDetailFileName);
            #endregion
        }

        #endregion
        #endregion

        #region 实现IIndicatorContext
        /// <summary>
        /// 取得K线
        /// </summary>
        /// <param name="code"></param>
        /// <param name="timeunit">时间单位</param>
        /// <param name="year">年份，为0表示取得所有</param>
        /// <returns></returns>
        public KLine GetKline(String code, TimeUnit timeunit = TimeUnit.day, int year = 0)
        {
            StrategyDataSet ds = this.dataset.FirstOrDefault(x => x.code == code);
            if (ds == null)
                return null;
            if (ds.klineDay == null)
                ds.LoadDayKLine();
            return ds.klineDay;
        }
        /// <summary>
        /// 取得资金动向指标
        /// </summary>
        /// <param name="code"></param>
        /// <param name="timeunit"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public MovementOfFunds GetMovementOfFunds(String code, TimeUnit timeunit = TimeUnit.day, int year = 0)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 取得立体买卖指标
        /// </summary>
        /// <param name="code"></param>
        /// <param name="timeunit"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public TradingLine GetTradingLine(String code, TimeUnit timeunit = TimeUnit.day, int year = 0)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region 实际执行
        /// <summary>历史数据索引</summary>      
        public const int INDEX_HISTORY = 0;
        /// <summary>关注中数据索引</summary>      
        public const int INDEX_ATTENTION = 1;
        /// <summary>建仓中数据索引</summary>      
        public const int INDEX_SETUP = 2;
        /// <summary>持仓中数据索引</summary>      
        public const int INDEX_HOLD = 3;
        /// <summary>平仓中数据索引</summary>      
        public const int INDEX_CLOSE = 4;
        /// <summary>状态中文名，用于打印</summary>    
        public readonly static String[] statusNames = {"历史状态","关注中", "建仓中", "持仓中", "平仓中" };
        /// <summary>状态文件名</summary>    
        public readonly static String[] statusFileNames = { "history.csv", "attention.csv","setup.csv", "hold.csv", "close.csv" };
        /// <summary>状态集</summary>    
        public readonly List<AlphaCodeStatus>[] alphastatus = new List<AlphaCodeStatus>[statusFileNames.Length];

        private String datapath;
        private String statuspath;
        private String codefilename;
        private log4net.ILog logger;
        private IndicatorRepository repository;
        private DateTime date;
        /// <summary>
        /// 实际执行
        /// </summary>
        public override void Run(Properties context)
        {
            //读取运行参数
            String dateStr = context.Get<String>("date");
            logger = (log4net.ILog)context.Get<Object>("logger");
            datapath = context.Get<String>("datapath");
            statuspath = context.Get<String>("statuspath");
            codefilename = context.Get<String>("codefilename");
            repository = (IndicatorRepository)context.Get<Object>("repository");
            //检查参数有效性
            if (dateStr == null || dateStr == "")                        
                dateStr = DateTime.Today.ToString("yyyyMMdd");            
            
            if (!DateUtils.TryParse(dateStr, out date))
                throw new Exception("上下文中的日期无效");
            datapath = FileUtils.GetDirectory(datapath);
            if (!File.Exists(datapath))
                throw new Exception("数据路径无效:"+datapath);
            statuspath = FileUtils.GetDirectory(statuspath);
            if (!File.Exists(statuspath))
                throw new Exception("状态路径无效:" + statuspath);
            if(repository == null)
                throw new Exception("行情库无效");
            //开始
            log.Info("启动策略执行系统:"+date.ToString("yyyyMMdd"));

            //读取股票代码
            List<String> codes = loadCodes(datapath, codefilename);
            if(codes == null || codes.Count<=0)
            {
                log.Info("没有需要运行的股票代码，本次执行结束");
                return;
            }
            log.Info("共读取到" + codes.Count+"个代码");

            //查找状态文件
            ReadStatusFile();
            //扫描数据
            scan(codes);
        }
        /// <summary>
        /// 读取状态文件
        /// </summary>
        public void ReadStatusFile()
        {
            for (int i = 0; i < statusFileNames.Length; i++)
            {
                alphastatus[i] = new List<AlphaCodeStatus>();
                String filename = statuspath + statusFileNames[i];
                if (!File.Exists(filename))
                {
                    logger.Info(statusNames[i] + "状态记录没有不存在");
                    continue;
                }

                String[] lines = File.ReadAllLines(filename, Encoding.UTF8);
                foreach (String line in lines)
                {
                    AlphaCodeStatus s = AlphaCodeStatus.Parse(line);
                    if (s != null) alphastatus[i].Add(s);
                }
                logger.Info("共有" + alphastatus[i].Count + "个" + statusNames[i] + "记录");
            }
        }
        /// <summary>
        /// 行情扫描
        /// </summary>
        public void scan(List<String> codes)
        {
            foreach (String code in codes)
                scan(code);                
        }
        /// <summary>
        /// 行情扫描
        /// </summary>
        /// <param name="code"></param>
        public void scan(String code)
        {
            //如果股票在关注集合中,检查它是否可以建仓
            AlphaCodeStatus statu = alphastatus[INDEX_ATTENTION].FirstOrDefault(x => x.Code == code);
            if (statu != null)
            {
                processAttention(code,statu);
                return;
            }
            //如果股票在建仓集合中，跳过
            statu = alphastatus[INDEX_SETUP].FirstOrDefault(x => x.Code == code);
            if (statu != null)
                return;

            //如果股票在持仓集合中，检查是否需要平仓
            statu = alphastatus[INDEX_HOLD].FirstOrDefault(x => x.Code == code);
            if(statu != null)
            {
                processHold(code,statu);
                return;
            }
            //如果股票在平仓集合中，跳过
            statu = alphastatus[INDEX_CLOSE].FirstOrDefault(x => x.Code == code);
            if (statu != null)
                return;

            //股票不在任何集合中，判断是否要进入关注

        }
        /// <summary>
        /// 处理关注阶段的股票
        /// </summary>
        /// <param name="code"></param>
        /// <param name="statu"></param>
        private void processAttention(String code, AlphaCodeStatus statu)
        {
            TimeSerialsDataSet ds = repository[code];
            if (ds == null)
            {
                logger.Info("股票" + code + "缺少时序数据集");
                return;
            }
            TimeSeries<ITimeSeriesItem<List<double>>> fundMains = ds.FundTrendCreateOrLoad();
            if (fundMains == null)
            {
                logger.Info("股票" + code + "缺少资金动向数据");
                return;
            }
            ITimeSeriesItem<List<double>> fundMainsItem = fundMains[date];
            if (fundMainsItem == null)
            {
                logger.Info("股票" + code + "缺少" + date.ToString("yyyyMMd") + "资金动向数据项");
                return;
            }
            if (fundMainsItem.Value[0] < p_mainforcelow)
            {
                logger.Info("股票" + code + "主力线仍低于" + p_mainforcelow.ToString("F3"));
                return;
            }
            logger.Info("股票" + code + "主力线上升通过" + p_mainforcelow.ToString("F3") + "，可以买入");

            if (ds.DayKLine == null)
            {
                logger.Info("股票" + code + "主力线上升通过,但缺少K线数据，无法买入");
                return;
            }
            KLineItem klineItem = ds.DayKLine[date];
            if (klineItem == null)
            {
                logger.Info("股票" + code + "主力线上升通过,但缺少当日K线数据，无法买入");
                return;
            }
            statu.SetupDate = date;
            statu.SetupFundMain = fundMainsItem.Value[0];
            statu.SetupPrice = klineItem.CLOSE;
            statu.SetupAmount = (int)(p_fundpergetin.Value / statu.SetupPrice);
            statu.Stage = AlphaStage.EntrustBuy;

            this.alphastatus[INDEX_ATTENTION].Remove(statu);
            this.alphastatus[INDEX_SETUP].Add(statu);
            saveAlphaStatus(INDEX_ATTENTION);
            saveAlphaStatus(INDEX_SETUP);

        }
        /// <summary>
        /// 处理持仓
        /// </summary>
        /// <param name="code"></param>
        /// <param name="statu"></param>
        private void processHold(String code,AlphaCodeStatus statu)
        {
            TimeSerialsDataSet ds = repository[code];
            KLineItem klineItem = ds==null?null:ds.DayKLine[date];
            
            int dayCounts = CalendarUtils.WorkDayCount(statu.TradeBout.BuyInfo.TradeDate.Date, date.Date);
            if (dayCounts >= p_maxholddays)
            {
                statu.Stage = AlphaStage.EntrustSell;
                statu.CloseDate = date;
                statu.ClosePrice = (klineItem == null ? 0 : klineItem.CLOSE);
                statu.CloseReason = "持仓超过"+ p_maxholddays.ToString()+"个工作日";
                this.alphastatus[INDEX_HOLD].Remove(statu);
                this.alphastatus[INDEX_CLOSE].Add(statu);
                saveAlphaStatus(INDEX_HOLD);
                saveAlphaStatus(INDEX_CLOSE);
            }
        }
        /// <summary>
        /// 处理空闲股票
        /// </summary>
        /// <param name="code"></param>
        private void processIdle(String code)
        {
            TimeSerialsDataSet ds = repository[code];
            if (ds == null)
            {
                logger.Info("股票" + code + "缺少时序数据集");
                return;
            }
            KLineItem klineItem = ds.DayKLine[date];
            if (klineItem == null)
            {
                logger.Info("股票" + code + "缺少当日K线数据，跳过");
                return;
            }
            TimeSeries<ITimeSeriesItem<List<double>>> fundMains = ds.FundTrendCreateOrLoad();
            if (fundMains == null)
            {
                logger.Info("股票" + code + "缺少资金动向数据");
                return;
            }
            ITimeSeriesItem<List<double>> fundMainsItem = fundMains[date];
            if (fundMainsItem == null)
            {
                logger.Info("股票" + code + "缺少" + date.ToString("yyyyMMd") + "资金动向数据项");
                return;
            }
            if (fundMainsItem.Value[0] >= p_mainforcelow)
            {
                logger.Info("股票" + code + "主力线仍高于" + p_mainforcelow.ToString("F3"));
                return;
            }
            AlphaCodeStatus status = new AlphaCodeStatus();
            status.Stage = AlphaStage.EnterLow;
            status.EnterLowDate = date;
            status.EnterlowFundMain = fundMainsItem.Value[0];
            this.alphastatus[INDEX_ATTENTION].Add(status);
            saveAlphaStatus(INDEX_ATTENTION);
        }
        /// <summary>
        /// 保存状态数据
        /// </summary>
        /// <param name="status"></param>
        private void saveAlphaStatus(int index)
        {
            List<AlphaCodeStatus> status = alphastatus[index];
            if (status == null)
                return;
            List<String> lines = new List<string>();
            for(int i=0;i<status.Count;i++)
            {
                lines.Add(status[i].ToString());
            }
            File.WriteAllLines(statuspath+statusFileNames[index],lines.ToArray(),Encoding.UTF8);
            logger.Info("保存"+statusNames[index]+"完成");
        }
        /// <summary>
        /// 事件触发动作
        /// </summary>
        /// <param name="context"></param>
        /// <param name="args"></param>
        public override void DoAction(IStrategyContext context, EventArgs args)
        {
            throw new NotImplementedException();
        }

        #endregion
    }


}
