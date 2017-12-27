using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using insp.Utility.Bean;
using insp.Security.Data;
using insp.Security.Data.kline;
using insp.Utility.Collections.Time;
using insp.Utility.Common;
using insp.Security.Data.Indicator;

namespace insp.Security.Strategy.Alpha.Sell
{
    public class DoBuyerFundMainforce : Buyer
    {
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


        private BacktestParameter backtestParameter;

        #endregion


        public override List<TradeBout> Execute(string code, Properties strategyParam, BacktestParameter backtestParam, ISeller seller = null)
        {
            IndicatorRepository repository = (IndicatorRepository)backtestParam.Get<Object>("repository");
            if (repository == null) return null;

            this.backtestParameter = backtestParam;

            p_mainforcelow = strategyParam.Get<int>("mainforcelow");
            p_mainforcerough = strategyParam.Get<int>("mainforcerough");

            p_fundpergetin = GetInMode.Parse(strategyParam.Get<String>("getinMode"));
            p_buypointdays = strategyParam.Get<int>("buypointdays");
            p_maxbuynum = strategyParam.Get<int>("maxbuynum");

            //创建数据集
            TimeSerialsDataSet ds = repository[code];            
            if (ds == null)
                return null;
            KLine klineDay = ds.DayKLine;
            if (klineDay == null || klineDay.Count < 0)
                return null;
            TimeSeries<ITimeSeriesItem<List<double>>> fundDay = ds.DayFundTrend;            
            if (fundDay == null || fundDay.Count<=0)
                return null;

            TradingLine tradeLineDay = ds.DayTradeLine;
            //买卖线数据无效则跳过
            if (tradeLineDay == null ||
               tradeLineDay.buyLine == null || tradeLineDay.buyLine.Count <= 0 ||
               tradeLineDay.sellLine == null || tradeLineDay.sellLine.Count <= 0 ||
               tradeLineDay.buysellPoints == null || tradeLineDay.buysellPoints.Count <= 0)
                return null;

            TradeRecords tradeRecords = new TradeRecords();

           
            


            //执行回测
            for (DateTime d = backtestParam.BeginDate; d <= backtestParam.EndDate; d = d.AddDays(1))
            {
                //if (!CalendarUtils.IsWorkDay(d))
                //    continue;

                KLineItem dayLineItem = klineDay[d];
                ITimeSeriesItem<List<double>> funds = fundDay[d];
                if (funds == null) continue;
                
                TradeBout buyBout = doTestBuy(d, tradeRecords,ds, dayLineItem, funds);
                if (buyBout != null)
                    d = buyBout.BuyInfo.TradeDate;
            }

            return tradeRecords.Bouts;
        }

        /// <summary>
        /// 回测买入
        /// </summary>
        /// <param name="d"></param>
        /// <param name="ds"></param>
        /// <param name="dayLineItem"></param>
        /// <param name="funds"></param>
        /// <returns></returns>
        private TradeBout doTestBuy(DateTime d, TradeRecords tradeRecords,TimeSerialsDataSet ds, KLineItem dayLineItem = null, ITimeSeriesItem<List<double>> funds = null)
        {
            if (dayLineItem == null)
                dayLineItem = ds.DayKLine[d];
            if (funds == null)
                funds = ds.DayFundTrend[d];

            //该股票有持仓的跳过
            if (p_maxbuynum > 0 && (tradeRecords.Count - tradeRecords.CountCompleted) >= p_maxbuynum)
                return null;
            
            //主力线不在低位跳过
            if (funds.Value[0] > p_mainforcelow)
                return null;


            TradeBout result = null;
            //寻找资金主力线突破位置 
            DateTime tDate;
            ITimeSeriesItem<List<double>> boundFundItem = ds.DayFundTrend.FindBoundayLineValue(funds.Date, p_mainforcelow, p_mainforcerough, comparasion, out tDate);
            if (boundFundItem == null)
                return null;           
            String reason = "主力线=" + boundFundItem.Value[0];

            d = boundFundItem.Date;
            funds = boundFundItem;
            dayLineItem = ds.DayKLine[d];

            //寻找对应买卖点
            ITimeSeriesItem<char> rt = null;
            if (p_buypointdays > 0)
            {
                DateTime td1 = d - new TimeSpan(p_buypointdays, 0, 0, 0, 0);
                DateTime td2 = d + new TimeSpan(p_buypointdays, 0, 0, 0, 0);
                if (ds.DayTradeLine.buysellPoints != null)
                {
                    TimeSeries<ITimeSeriesItem<char>> ts = ds.DayTradeLine.buysellPoints[td1, td2];
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
                TradeBout newBout = new TradeBout(ds.Code);
                newBout.RecordTrade(1, d, TradeDirection.Buy, price, amount, backtestParameter.Volumecommission, 0, reason);
                tradeRecords.Bouts.Add(newBout);
                return newBout;
            }
            return null;
        }
    }
}
