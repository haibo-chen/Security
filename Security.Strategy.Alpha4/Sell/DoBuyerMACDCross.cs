using insp.Security.Data;
using insp.Security.Data.Indicator.Macd;
using insp.Security.Data.kline;
using insp.Utility.Bean;
using insp.Utility.Collections.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Strategy.Alpha.Sell
{
    public class DoBuyerMACDCross : Buyer
    {
        public override List<TradeInfo> DoBuy(Properties strategyParams, DateTime d, StrategyContext context)
        {
            throw new NotImplementedException();
        }

        public override TradeRecords Execute(String code,Properties strategyParam, BacktestParameter backtestParam,ISeller seller = null)
        {
            IndicatorRepository repository = (IndicatorRepository)backtestParam.Get<Object>("repository");
            if (repository == null) return null;
            //取得策略参数           
            double buy_mainlow = strategyParam.Get<double>("buy_mainlow"); //主力线低位买入
            int buy_cross = strategyParam.Get<int>("buy_cross");
            GetInMode p_getinMode = (GetInMode)strategyParam.Get<GetInMode>("getinMode");

            //取得行情数据
            TradeRecords tr = new TradeRecords(code);
            TimeSerialsDataSet ds = repository[code];
            if (ds == null) return null;

            KLine kline = ds.DayKLine;
            if (kline == null) return null;

            MACD macd = (MACD)ds.Create("macd", TimeUnit.day, false);
            if (macd == null) return null;

            //买入条件判定
            for (int i = 0; i < macd.Count; i++)
            {
                MACDItem macdItem = macd[i];
                if (macdItem.Date < backtestParam.BeginDate || macdItem.Date >= backtestParam.EndDate)
                    continue;

                if (macdItem.CROSS <= 0)
                    continue;

                if (macdItem.DIF > buy_mainlow)
                    continue;

                DateTime d = macdItem.Date;
                KLineItem klineItem = kline[d];
                if (klineItem == null) continue;
                TradeBout bout = new TradeBout(code);
                bout.RecordTrade(1, d, TradeDirection.Buy, klineItem.CLOSE, (int)(p_getinMode.Value / klineItem.CLOSE), backtestParam.Volumecommission, backtestParam.Stampduty, "低位金叉" + macdItem.DIF.ToString("F2"));
                tr.Bouts.Add(bout);

            }
            return tr;

        }
    }
}
