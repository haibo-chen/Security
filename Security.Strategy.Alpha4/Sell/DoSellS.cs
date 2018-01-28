using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using insp.Utility.Bean;
using insp.Security.Data;
using insp.Utility.Collections.Time;
using insp.Security.Data.kline;
using insp.Utility.Collections;
using insp.Utility.Date;

namespace insp.Security.Strategy.Alpha.Sell
{
    public class DoSellS : Seller
    {
        public override bool DoSell(string code, TradeBout bout, DateTime d, Properties strategyParam, BacktestParameter backtestParam, out string reason)
        {
            reason = "";
            if (bout == null) return false;
            IndicatorRepository repository = (IndicatorRepository)backtestParam.Get<Object>("repository");
            if (repository == null) return false;

            TimeSerialsDataSet ds = repository[bout.Code];
            if (ds == null) return false;
            KLine kline = ds.DayKLine;
            if (kline == null) return false;

            //跳过已完成的
            if (bout.Completed) return false;

            TimeSeries<ITimeSeriesItem<char>> btpoints = ds.DayTradeLine.buysellPoints;
            if (btpoints == null || btpoints.Count <= 0)
                return false;

            ITimeSeriesItem<char> item = btpoints[d];
            if (item == null || item.Value == 'B')
                return false;

            KLineItem klineItemDay = kline[d];
            if (klineItemDay == null) return false;

            double price = klineItemDay.CLOSE;
            //if (price < bout.BuyInfo.TradePrice)
            //    return true;
            
            int amount = bout.BuyInfo.Amount;
            bout.RecordTrade(2, klineItemDay.Date, TradeDirection.Sell, price, amount, backtestParam.Volumecommission, backtestParam.Stampduty, "日线出S点");

            return false;

        }

        public override TradeInfo DoSell(HoldRecord holdRecord, DateTime d, Properties strategyParams, StrategyContext context)
        {
            throw new NotImplementedException();
        }
    }
}
