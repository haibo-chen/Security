using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using insp.Utility.Bean;
using insp.Security.Data;
using insp.Security.Data.kline;
using insp.Utility.Collections.Time;

namespace insp.Security.Strategy.Alpha.Sell
{
    /// <summary>
    /// 根据主力资金线买入
    /// </summary>
    public class DoBuyerFundLine : Buyer
    {
        public override List<TradeBout> Execute(string code, Properties strategyParam, BacktestParameter backtestParam, ISeller seller = null)
        {
            IndicatorRepository repository = (IndicatorRepository)backtestParam.Get<Object>("repository");
            //取得策略参数
            double buy_mainlow = strategyParam.Get<double>("buy_mainlow"); //主力线低位买入
            int buy_cross = strategyParam.Get<int>("buy_cross");
            GetInMode p_getinMode = (GetInMode)strategyParam.Get<GetInMode>("getinMode");


            List<TradeBout> bouts = new List<TradeBout>();
            TimeSerialsDataSet ds = repository[code];
            if (ds == null) return null;

            KLine kline = ds.DayKLine;
            if (kline == null) return null;
            TimeSeries<ITimeSeriesItem<List<double>>> dayFunds = ds.DayFundTrend;
            TimeSeries<ITimeSeriesItem<double>> dayFundsCross = ds.DayFundTrendCross;

            
            if (buy_cross == 0 && dayFunds == null)
                return null;
            else if (buy_cross == 1 && (dayFundsCross == null || dayFunds == null))
                return null;

            #region 判断主力线低位决定买入点
            if (buy_cross == 0)
            {
                for (int i = 0; i < dayFunds.Count; i++)
                {
                    if (dayFunds[i].Date.Date < backtestParam.BeginDate || dayFunds[i].Date.Date > backtestParam.EndDate)
                        continue;
                    if (double.IsNaN(dayFunds[i].Value[0]))
                        continue;
                    if (dayFunds[i].Value[0] > buy_mainlow)
                        continue;
                    //主力线开始低于buy_mainlow...
                    i += 1;
                    while (i < dayFunds.Count)
                    {
                        if (dayFunds[i].Value[0] <= buy_mainlow)
                        {
                            i += 1;
                            continue;
                        }
                        //主力线出了buy_mainlow
                        KLineItem klineItem = kline[dayFunds[i].Date];
                        if (klineItem == null) break;
                        int tIndex = kline.IndexOf(klineItem);
                        if (tIndex >= kline.Count - 1) break;
                        KLineItem klineItemNext = kline[tIndex + 1];
                        TradeBout bout = new TradeBout(code);
                        double price = klineItem.CLOSE;
                        if (price > klineItemNext.HIGH || price < klineItemNext.LOW)
                            break;
                        bout.RecordTrade(1, dayFunds[i].Date.Date, TradeDirection.Buy, price, (int)(p_getinMode.Value / price), backtestParam.Volumecommission, backtestParam.Stampduty, "主力线低于" + buy_mainlow.ToString("F2"));
                        bouts.Add(bout);
                        break;
                    }
                }
            }
            #endregion

            #region 判断金叉决定买入点
            else if (buy_cross == 1)
            {
                for (int i = 0; i < dayFundsCross.Count; i++)
                {
                    if (dayFundsCross[i].Date.Date < backtestParam.BeginDate || dayFundsCross[i].Date.Date > backtestParam.EndDate)
                        continue;
                    if (dayFundsCross[i].Value <= 0)
                        continue;
                    ITimeSeriesItem<List<double>> dayFundItem = dayFunds[dayFundsCross[i].Date];
                    if (dayFundItem == null) continue;
                    if (buy_mainlow != 0 && dayFundItem.Value[0] >= buy_mainlow) continue;

                    KLineItem klineItem = kline[dayFundItem.Date];
                    if (klineItem == null) continue;
                    int tIndex = kline.IndexOf(klineItem);
                    if (tIndex >= kline.Count - 1) continue;
                    KLineItem klineItemNext = kline[tIndex + 1];
                    TradeBout bout = new TradeBout(code);
                    double price = klineItem.CLOSE;
                    if (price > klineItemNext.HIGH || price < klineItemNext.LOW)
                        continue;
                    bout.RecordTrade(1, dayFunds[i].Date.Date, TradeDirection.Buy, price, (int)(p_getinMode.Value / price), backtestParam.Volumecommission, backtestParam.Stampduty, "主力线低于" + buy_mainlow.ToString("F2"));
                    bouts.Add(bout);
                }
            }
            #endregion

            return bouts;
        }
    }
}
