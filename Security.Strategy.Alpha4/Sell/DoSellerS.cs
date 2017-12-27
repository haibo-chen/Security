using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using insp.Utility.Bean;
using insp.Utility.Collections.Time;
using insp.Security.Data.kline;
using insp.Security.Data;

namespace insp.Security.Strategy.Alpha.Sell
{
    /// <summary>
    /// 日线S点卖出
    /// </summary>
    public class DoSellerS : Seller
    {
        public override void Execute(List<TradeBout> bouts, Properties strategyParam, BacktestParameter backtestParam)
        {
            IndicatorRepository repository = (IndicatorRepository)backtestParam.Get<Object>("repository");
            if (repository == null) return;
            
            foreach (TradeBout bout in bouts)
            {                
                TimeSerialsDataSet ds = repository[bout.Code];
                if (ds == null) continue;

                TimeSeries<ITimeSeriesItem<char>> dayTradePt = ds.CubePtCreateOrLoad();
                if (dayTradePt == null)
                    return;
                if (bouts == null || bouts.Count <= 0)
                    return;
                KLine dayLine = ds.DayKLine;
                if (dayLine == null)
                    return;

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
}
