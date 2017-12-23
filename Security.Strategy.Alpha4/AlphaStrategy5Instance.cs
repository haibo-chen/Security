using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class AlphaStrategy5Instance : StrategyInstance
    {
        
        #region 初始化
        /// <summary>
        /// 版本
        /// </summary>
        public override Version Version { get { return new Version(5, 0, 0); } }


        
        

        /// <summary>
        /// 构造方法
        /// </summary>
        internal AlphaStrategy5Instance(Properties props) { id = "1"; this.props = props; }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="id"></param>
        internal AlphaStrategy5Instance(String id, Properties props) { this.id = id; this.props = props; }

        

        #endregion

        #region 回测
       
        

        protected override List<TradeBout> doTestByCodes(List<String> codes)
        {
            String modeName = props.Get<String>("mode");
            Mode mode = Mode.Find(modeName);
            log.Info("执行模式=" + mode.Name);

            IndicatorRepository repository = (IndicatorRepository)backtestParam.Get<Object>("repository");

            DateTime begin = backtestParam.BeginDate;
            DateTime end = backtestParam.EndDate;

            List<TradeBout> allbouts = new List<TradeBout>();
            foreach (String code in codes)
            {
                //生成交易数据
                TimeSerialsDataSet ds = repository[code];
                if (ds == null) continue;

                List<TradeBout> bouts = mode.DoBuy(ds, props, backtestParam);
                mode.DoSell(bouts, ds, props, backtestParam);
                RemoveUnCompeletedBouts(bouts);
                if (bouts != null && bouts.Count > 0)
                    allbouts.AddRange(bouts);
                if(bouts !=null && bouts.Count > 0)
                {
                    double totalProfilt = allbouts.Sum(x => x.Profit);
                    double totalCost = allbouts.Sum(x => x.BuyInfo.TradeCost);
                    log.Info(ds.Code + ":回合数=" + bouts.Count.ToString() +
                                       ",胜率=" + (bouts.Count(x => x.Win) * 1.0 / bouts.Count).ToString("F3") +
                                       ",盈利=" + bouts.Sum(x => x.Profit).ToString("F2") +
                                       ",总胜率=" + (allbouts.Count(x => x.Win) * 1.0 / allbouts.Count).ToString("F3") +
                                       ",总盈利=" + totalProfilt.ToString("F2") +
                                       ",平均盈利率=" + (totalProfilt/ totalCost).ToString("F3"));

                }
                    
            }
            return allbouts;
        }
        #endregion

        #region 大盘判断

        private GrailParameter p_grail;
        private void grailInit()
        {
            p_grail = GrailParameter.Parse(this.props.Get<String>("grail"));
            IndicatorRepository repository = (IndicatorRepository)backtestParam.Get<Object>("repository");
            p_grail.Init(repository);
        }
        /// <summary>
        /// 特定日期禁止买入
        /// </summary>
        /// <param name="d"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        protected override bool isForbidBuy(DateTime d, String code, out String reason)
        {
            reason = "";
            if (p_grail == null)
                grailInit();
            if (p_grail == null || !p_grail.Enable)
                return false;
            return p_grail.CanBuy(d, code);
        }
        /// <summary>
        /// 特定日期禁止持仓
        /// </summary>
        /// <param name="d"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        protected override bool isForbidHold(DateTime d, String code, out String reason)
        {
            reason = "";
            if (p_grail == null)
                grailInit();
            if (p_grail == null || !p_grail.Enable)
                return false;
            if(p_grail.MustSell(d,code))
            {
                reason = "大盘发出S或未到买点";
                return true;
            }
            return false;
        }

        #endregion
        #region 不同的买入模式

        internal abstract class Mode
        {                        
            public virtual String Name { get; }
            public static readonly List<Mode> MODES = CollectionUtils.AsList((Mode)new ModeA(), (Mode)new ModeB(), (Mode)new ModeC(), (Mode)new ModeD());
            public static Mode Find(String name)
            {
                return MODES.FirstOrDefault(x => x.Name == name);
            }

            
            public abstract List<TradeBout> DoBuy(TimeSerialsDataSet ds, Properties strategyParam, BacktestParameter backtestParam);

            public virtual void DoSell(List<TradeBout> bouts, TimeSerialsDataSet ds, Properties strategyParam, BacktestParameter backtestParam)
            {
                DoSell1(bouts,ds,strategyParam,backtestParam);
            }
            /// <summary>
            /// 根据个股主力线高位卖出
            /// </summary>
            /// <param name="bouts"></param>
            /// <param name="ds"></param>
            /// <param name="strategyParam"></param>
            /// <param name="backtestParam"></param>
            public void DoSell2(List<TradeBout> bouts, TimeSerialsDataSet ds, Properties strategyParam, BacktestParameter backtestParam)
            {
                if (bouts == null || bouts.Count <= 0)
                    return;
                TimeSeries<ITimeSeriesItem<List<double>>> dayFunds = ds.DayFundTrend;
                KLine dayLine = ds.DayKLine;
                if (dayLine == null) return;

                foreach (TradeBout bout in bouts)
                {
                    DateTime buyDate = bout.BuyInfo.TradeDate;
                    //找20个工作日的收盘价最高值
                    KLineItem klineItem = dayLine.GetNearest(buyDate, false);
                    if (klineItem == null) continue;
                    int index = dayLine.IndexOf(klineItem);
                    DateTime sellDate = buyDate;
                    double sellPrice = 0;
                    for(int i=index+1;i< Math.Min(index+41,dayLine.Count);i++)
                    {
                        if (dayLine[i].CLOSE > sellPrice)
                        {
                            sellPrice = dayLine[i].CLOSE;
                            sellDate = dayLine[i].Date;
                        }
                    }
                    bout.RecordTrade(2, sellDate, TradeDirection.Sell, sellPrice, bout.BuyInfo.Amount, 0, 0, "");

                }
            }
            /// <summary>
            /// 根据个股S点卖出
            /// </summary>
            /// <param name="bouts"></param>
            /// <param name="ds"></param>
            /// <param name="strategyParam"></param>
            /// <param name="backtestParam"></param>
            public void DoSell1(List<TradeBout> bouts, TimeSerialsDataSet ds, Properties strategyParam, BacktestParameter backtestParam)
            {
                TimeSeries<ITimeSeriesItem<char>> dayTradePt = ds.CubePtCreateOrLoad();
                if (dayTradePt == null)
                    return;
                if (bouts == null || bouts.Count <= 0)
                    return;
                KLine dayLine = ds.DayKLine;
                if (dayLine == null)
                    return;
                foreach (TradeBout bout in bouts)
                {
                    DateTime buyDate = bout.BuyInfo.TradeDate;
                    KeyValuePair<int, ITimeSeriesItem> dayTradePtItem = dayTradePt.GetNearest(buyDate, false);
                    if (dayTradePtItem.Key < 0)
                        continue;
                    if (dayTradePtItem.Value == null) continue;
                    int index = dayTradePt.IndexOf(dayTradePtItem.Value.Date);
                    for (int k = index; k < dayTradePt.Count; k++)
                    {
                        if (dayTradePt[k].Value == 'S')
                        {
                            KLineItem dayLineItem = dayLine[dayTradePt[k].Date];
                            if (dayLineItem == null) break;
                            bout.RecordTrade(2, dayLineItem.Date, TradeDirection.Sell, dayLineItem.CLOSE, bout.BuyInfo.Amount, 0, 0, "发S点");
                            break;
                        }
                    }

                }
            }
        }
        internal class ModeA : Mode
        {
            public override String Name { get { return "日线金叉;周线偏离"; } }

            public override List<TradeBout> DoBuy(TimeSerialsDataSet ds,Properties strategyParam,BacktestParameter backtestParam)
            {
                TimeSeries<ITimeSeriesItem<List<double>>> dayFunds = ds.FundTrendCreateOrLoad(TimeUnit.day);
                TimeSeries<ITimeSeriesItem<List<double>>> weekFunds = ds.FundTrendCreateOrLoad(TimeUnit.week);
                TimeSeries<ITimeSeriesItem<double>> dayCross = ds.FundTrendCrossCreateOrLoad(TimeUnit.day);
                TimeSeries<ITimeSeriesItem<double>> weedCross = ds.FundTrendCrossCreateOrLoad(TimeUnit.day);

                if (dayFunds == null || dayFunds.Count <= 0 || weekFunds == null || weekFunds.Count <= 0 || dayCross == null || dayCross.Count <= 0 || weedCross == null || weedCross.Count <= 0)
                    return null;

                List<TradeBout> bouts = new List<TradeBout>();
                DateTime begin = backtestParam.BeginDate;
                DateTime end = backtestParam.EndDate;
                double p_day_low = strategyParam.Get<double>("day_low");
                double p_day_bias = strategyParam.Get<double>("day_bias");
                double p_week_low = strategyParam.Get<double>("week_low");
                double p_week_bias = strategyParam.Get<double>("week_bias");
                GetInMode p_getinMode = (GetInMode)strategyParam.Get<GetInMode>("fundpergetin");
                for (int i = 0; i < dayCross.Count; i++)
                {
                    ITimeSeriesItem<double> dayCrossItem = dayCross[i];
                    if (dayCrossItem == null) continue;                    
                    if (dayCrossItem.Date < begin || dayCrossItem.Date >= end)
                        continue;
                    if (dayCrossItem.Value <= 0)
                        continue;
                    ITimeSeriesItem<List<double>> dayFundItem = dayFunds[dayCrossItem.Date];
                    if (p_day_low != 0 && dayFundItem.Value[0] > p_day_low)//日主力线金叉点不在低位
                        continue;

                    DateTime td = CalendarUtils.GetWeek(dayCrossItem.Date, DayOfWeek.Friday);
                    ITimeSeriesItem<List<double>> weekFundItem = weekFunds[td];
                    if (weekFundItem == null)
                        continue;
                    if (p_week_bias != 0 && ((weekFundItem.Value[0] - weekFundItem.Value[1]) < p_week_bias)) continue;

                    KLine dayLine = ds.DayKLine;
                    if (dayLine == null) continue;
                    KLineItem dayLineItem = dayLine[dayCrossItem.Date];
                    if (dayLineItem == null) continue;

                    TradeBout bout = new TradeBout(ds.Code);
                    bout.RecordTrade(1, dayCrossItem.Date, TradeDirection.Buy, dayLineItem.CLOSE, (int)(p_getinMode.Value / dayLineItem.CLOSE), 0, 0, Name);
                    bouts.Add(bout);
                }
                return bouts;
            }

            

        }
        internal class ModeB : Mode
        {
            public override String Name { get { return "日线金叉;周线金叉"; } }

            public override List<TradeBout> DoBuy(TimeSerialsDataSet ds, Properties strategyParam, BacktestParameter backtestParam)
            {
                TimeSeries<ITimeSeriesItem<List<double>>> dayFunds = ds.FundTrendCreateOrLoad(TimeUnit.day);
                TimeSeries<ITimeSeriesItem<List<double>>> weekFunds = ds.FundTrendCreateOrLoad(TimeUnit.week);
                TimeSeries<ITimeSeriesItem<double>> dayCross = ds.FundTrendCrossCreateOrLoad(TimeUnit.day);
                TimeSeries<ITimeSeriesItem<double>> weedCross = ds.FundTrendCrossCreateOrLoad(TimeUnit.day);

                if (dayFunds == null || dayFunds.Count <= 0 || weekFunds == null || weekFunds.Count <= 0 || dayCross == null || dayCross.Count <= 0 || weedCross == null || weedCross.Count <= 0)
                    return null;

                List<TradeBout> bouts = new List<TradeBout>();
                DateTime begin = backtestParam.BeginDate;
                DateTime end = backtestParam.EndDate;
                double p_day_low = strategyParam.Get<double>("day_low");
                double p_day_bias = strategyParam.Get<double>("day_bias");
                double p_week_low = strategyParam.Get<double>("week_low");
                double p_week_bias = strategyParam.Get<double>("week_bias");
                GetInMode p_getinMode = GetInMode.Parse(strategyParam.Get<String>("fundpergetin"));
                for (int i = 0; i < dayCross.Count; i++)
                {
                    ITimeSeriesItem<double> dayCrossItem = dayCross[i];
                    if (dayCrossItem == null) continue;
                    if (dayCrossItem.Date < begin || dayCrossItem.Date >= end)
                        continue;
                    if (dayCrossItem.Value < 0) continue;
                    if (p_day_low!=0 && dayCrossItem.Value > p_day_low)
                        continue;

                    DateTime td = CalendarUtils.GetWeek(dayCrossItem.Date, DayOfWeek.Friday);
                    ITimeSeriesItem<double> weekCrossItem1 = weedCross[td];
                    ITimeSeriesItem<double> weekCrossItem2 = weedCross[td.AddDays(-7)];
                    if (weekCrossItem1 == null && weekCrossItem2 == null)
                        continue;
                    if (p_week_low!=0 && (weekCrossItem1 != null && weekCrossItem1.Value > p_week_low && weekCrossItem2 != null && weekCrossItem2.Value > p_week_low)) continue;

                    KLine dayLine = ds.DayKLine;
                    if (dayLine == null) continue;
                    KLineItem dayLineItem = dayLine[dayCrossItem.Date];
                    if (dayLineItem == null) continue;

                    TradeBout bout = new TradeBout(ds.Code);
                    bout.RecordTrade(1, dayCrossItem.Date, TradeDirection.Buy, dayLineItem.CLOSE, (int)(p_getinMode.Value / dayLineItem.CLOSE), 0, 0, Name);
                    bouts.Add(bout);
                }
                return bouts;
            }
        }
        internal class ModeC : Mode
        {
            public override String Name { get { return "日线偏离;周线金叉"; } }

            public override List<TradeBout> DoBuy(TimeSerialsDataSet ds, Properties strategyParam, BacktestParameter backtestParam)
            {
                TimeSeries<ITimeSeriesItem<List<double>>> dayFunds = ds.FundTrendCreateOrLoad(TimeUnit.day);
                TimeSeries<ITimeSeriesItem<List<double>>> weekFunds = ds.FundTrendCreateOrLoad(TimeUnit.week);
                TimeSeries<ITimeSeriesItem<double>> dayCross = ds.FundTrendCrossCreateOrLoad(TimeUnit.day);
                TimeSeries<ITimeSeriesItem<double>> weekCross = ds.FundTrendCrossCreateOrLoad(TimeUnit.day);

                if (dayFunds == null || dayFunds.Count <= 0 || weekFunds == null || weekFunds.Count <= 0 || dayCross == null || dayCross.Count <= 0 || weekCross == null || weekCross.Count <= 0)
                    return null;

                List<TradeBout> bouts = new List<TradeBout>();
                DateTime begin = backtestParam.BeginDate;
                DateTime end = backtestParam.EndDate;
                double p_day_low = strategyParam.Get<double>("day_low");
                double p_day_bias = strategyParam.Get<double>("day_bias");
                double p_week_low = strategyParam.Get<double>("week_low");
                double p_week_bias = strategyParam.Get<double>("week_bias");
                GetInMode p_getinMode = (GetInMode)strategyParam.Get<Object>("fundpergetin");
                for (int i = 0; i < dayFunds.Count; i++)
                {
                    ITimeSeriesItem<List<double>> dayFundItem = dayFunds[i];

                    if (dayFundItem == null) continue;
                    if (dayFundItem.Date < begin || dayFundItem.Date >= end)
                        continue;
                    if ((dayFundItem.Value[0] - dayFundItem.Value[1]) < p_day_bias) continue;

                    DateTime td = CalendarUtils.GetWeek(dayFundItem.Date, DayOfWeek.Friday);
                    ITimeSeriesItem<double> weekCrossItem = weekCross[td];
                    if (weekCrossItem == null)
                        continue;
                    if (weekCrossItem.Value > p_week_low) continue;

                    KLine dayLine = ds.DayKLine;
                    if (dayLine == null) continue;
                    KLineItem dayLineItem = dayLine[dayFundItem.Date];
                    if (dayLineItem == null) continue;

                    TradeBout bout = new TradeBout(ds.Code);
                    bout.RecordTrade(1, dayFundItem.Date, TradeDirection.Buy, dayLineItem.CLOSE, (int)(p_getinMode.Value / dayLineItem.CLOSE), 0, 0, Name);
                    bouts.Add(bout);
                }
                return bouts;
            }

        }
        internal class ModeD : Mode
        {
            public override String Name { get { return "日线偏离;周线偏离"; } }

            public override List<TradeBout> DoBuy(TimeSerialsDataSet ds, Properties strategyParam, BacktestParameter backtestParam)
            {
                TimeSeries<ITimeSeriesItem<List<double>>> dayFunds = ds.FundTrendCreateOrLoad(TimeUnit.day);
                TimeSeries<ITimeSeriesItem<List<double>>> weekFunds = ds.FundTrendCreateOrLoad(TimeUnit.week);
                TimeSeries<ITimeSeriesItem<double>> dayCross = ds.FundTrendCrossCreateOrLoad(TimeUnit.day);
                TimeSeries<ITimeSeriesItem<double>> weekCross = ds.FundTrendCrossCreateOrLoad(TimeUnit.day);

                if (dayFunds == null || dayFunds.Count <= 0 || weekFunds == null || weekFunds.Count <= 0 || dayCross == null || dayCross.Count <= 0 || weekCross == null || weekCross.Count <= 0)
                    return null;

                List<TradeBout> bouts = new List<TradeBout>();
                DateTime begin = backtestParam.BeginDate;
                DateTime end = backtestParam.EndDate;
                double p_day_low = strategyParam.Get<double>("day_low");
                double p_day_bias = strategyParam.Get<double>("day_bias");
                double p_week_low = strategyParam.Get<double>("week_low");
                double p_week_bias = strategyParam.Get<double>("week_bias");
                GetInMode p_getinMode = (GetInMode)strategyParam.Get<Object>("fundpergetin");
                for (int i = 0; i < dayFunds.Count; i++)
                {
                    ITimeSeriesItem<List<double>> dayFundItem = dayFunds[i];

                    if (dayFundItem == null) continue;
                    if (dayFundItem.Date < begin || dayFundItem.Date >= end)
                        continue;
                    if ((dayFundItem.Value[0] - dayFundItem.Value[1]) < p_day_bias) continue;

                    DateTime td = CalendarUtils.GetWeek(dayFundItem.Date, DayOfWeek.Friday);
                    ITimeSeriesItem<List<double>> weekFundItem = weekFunds[td];
                    if (weekFundItem == null)
                        continue;
                    if ((weekFundItem.Value[0] - weekFundItem.Value[1]) < p_week_bias) continue;

                    KLine dayLine = ds.DayKLine;
                    if (dayLine == null) continue;
                    KLineItem dayLineItem = dayLine[dayFundItem.Date];
                    if (dayLineItem == null) continue;

                    TradeBout bout = new TradeBout(ds.Code);
                    bout.RecordTrade(1, dayFundItem.Date, TradeDirection.Buy, dayLineItem.CLOSE, (int)(p_getinMode.Value / dayLineItem.CLOSE), 0, 0, Name);
                    bouts.Add(bout);
                }
                return bouts;
            }
        }

        #endregion

        #region 实际执行
        public override void Run(Properties context)
        {

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
