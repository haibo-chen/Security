using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using insp.Utility.Bean;
using insp.Security.Data;
using insp.Security.Data.kline;
using insp.Utility.Date;

namespace insp.Security.Strategy.Alpha.Sell
{
    public class DoSellerMaxProfilt : Seller
    {
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
        /// 执行
        /// </summary>
        /// <param name="bouts"></param>
        /// <param name="strategyParam"></param>
        /// <param name="backtestParam"></param>
        public override void Execute(List<TradeBout> bouts, Properties strategyParam, BacktestParameter backtestParam)
        {
            IndicatorRepository repository = (IndicatorRepository)backtestParam.Get<Object>("repository");
            if (repository == null) return;
           
            p_maxprofilt = strategyParam.Get<double>("maxprofilt");
            p_maxholddays = strategyParam.Get<int>("maxholddays");
            p_stoploss = strategyParam.Get<double>("stoploss");

            foreach (TradeBout bout in bouts)
            {
                TimeSerialsDataSet ds = repository[bout.Code];
                if (ds == null) continue;

                if (bout.Completed) continue;//跳过已完成的

                KLine kline = ds.DayKLine;
                if (kline == null) continue;

                int bIndex = kline.IndexOf(bout.BuyInfo.TradeDate);
                if (bIndex < 0) continue;

                int index = bIndex + 1;
                while (index < kline.Count)
                {
                    KLineItem dayLineItem = kline[index];
                    double diff = dayLineItem.HIGH - bout.BuyInfo.TradePrice;
                    double percent = diff / bout.BuyInfo.TradePrice;
                    if (percent >= p_maxprofilt) //盈利超过预定
                    {
                        double price = bout.BuyInfo.TradePrice * (1 + p_maxprofilt);
                        int amount = bout.BuyInfo.Amount;
                        bout.RecordTrade(2, dayLineItem.Date, TradeDirection.Sell, price, amount, backtestParam.Volumecommission, backtestParam.Stampduty, "盈利>=" + p_maxprofilt.ToString("F2"));
                        break;
                    }
                    else if (p_maxholddays != 0 && CalendarUtils.WorkDayCount(bout.BuyInfo.TradeDate, dayLineItem.Date) >= p_maxholddays) //持仓超过n天
                    {
                        bool selled = false;
                        //尝试15日内以不亏损的方式卖出                        
                        for (int k = 0; k < 15; k++)
                        {
                            if (kline.Count <= index + k + 1)
                                break;
                            KLineItem item = kline[index + k + 1];
                            if (item != null && item.HIGH >= bout.BuyInfo.TradePrice)
                            {
                                int tamount = bout.BuyInfo.Amount;
                                bout.RecordTrade(2, item.Date, TradeDirection.Sell, bout.BuyInfo.TradePrice, tamount, backtestParam.Volumecommission, backtestParam.Stampduty, "持仓超过" + p_maxholddays.ToString() + "后第" + (k + 1).ToString() + "日卖出");

                                selled = true;
                                break;
                            }
                        }
                        if (selled) break;

                        double price = dayLineItem.CLOSE;
                        int amount = bout.BuyInfo.Amount;
                        bout.RecordTrade(2, dayLineItem.Date, TradeDirection.Sell, price, amount, backtestParam.Volumecommission, backtestParam.Stampduty, "持仓超过" + p_maxholddays.ToString());

                    }
                    else if (p_stoploss > 0 && -1 * percent > p_stoploss)//达到止损线
                    {
                        double price = dayLineItem.CLOSE;
                        int amount = bout.BuyInfo.Amount;
                        bout.RecordTrade(2, dayLineItem.Date, TradeDirection.Sell, price, amount, backtestParam.Volumecommission, backtestParam.Stampduty, "到达止损线" + p_stoploss.ToString("F2"));
                        break;
                    }
                    index += 1;
                }
            }
        }
    }
}
