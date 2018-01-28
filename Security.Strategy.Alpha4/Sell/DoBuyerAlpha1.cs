using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using insp.Utility.Bean;
using insp.Security.Data;
using insp.Security.Data.kline;
using insp.Utility.Collections.Time;
using insp.Security.Data.Indicator;

namespace insp.Security.Strategy.Alpha.Sell
{
    public class DoBuyerAlpha1 : Buyer
    {
        public override List<TradeInfo> DoBuy(Properties strategyParam, DateTime d, StrategyContext context)
        {
            //取得行情库
            IndicatorRepository repository = (IndicatorRepository)context.Get<Object>("repository");
            if (repository == null) return null;

            //读取代码
            List<String> codes = LoadCodes(strategyParam, context);
            if (codes == null || codes.Count <= 0)
                return null;

            //取得策略参数            
            double p_mainforcelow = strategyParam.Get<double>("mainforcelow");
            int p_monthbutpt = strategyParam.Get<int>("monthbutpt", 0);
            //double p_mainforceclimb = strategyParam.Get<double>("mainforceclimb");
            double p_mainforceslope = strategyParam.Get<double>("mainforceslope");
            int p_mainforcerough = strategyParam.Get<int>("mainforcerough");
            int p_buypointdays = strategyParam.Get<int>("buypointdays");
            int p_maxbuynum = strategyParam.Get<int>("maxbuynum");
            GetInMode p_fundpergetin = GetInMode.Parse(strategyParam.Get<String>("getinMode"));
            GrailParameter p_grail = GrailParameter.Parse(strategyParam.Get<String>("grail"));
            double stampduty = context.Get<double>("stampduty");
            double volumecommission = context.Get<double>("volumecommission");

            List<TradeInfo> results = new List<TradeInfo>();
            //遍历
            foreach (String code in codes)
            {
                TimeSerialsDataSet ds = repository[code];
                if (ds == null) continue;
                KLine klineDay = ds.DayKLine;
                if (klineDay == null) continue;
                KLineItem klineItemDay = klineDay[d];
                if (klineItemDay == null) continue;

                TimeSeries<ITimeSeriesItem<List<double>>> fundDay = ds.DayFundTrend;
                if (fundDay == null) continue;
                ITimeSeriesItem<List<double>> fundItemDay = fundDay[d];
                if (fundItemDay == null) continue;
                int index = fundDay.IndexOf(fundItemDay);
                if (index <= 0) continue;
                ITimeSeriesItem<List<double>> prevfundItemDay = fundDay[index - 1];

                if (!p_grail.CanBuy(d, code)) //大盘禁止买入的跳过
                    continue;

                if (p_mainforcelow > 0)//判断主力线上穿p_mainforcelow
                {
                    if (fundItemDay.Value[0] < p_mainforcelow) continue;
                    if (prevfundItemDay.Value[0] > p_mainforcelow) continue;
                }

                if(p_mainforceslope > 0) //判断主力线上升速度超过p_mainforceslope
                {
                    if (fundItemDay.Value[0] - prevfundItemDay.Value[0] < p_mainforceslope)
                        continue;
                }

                TradeInfo tradeInfo = new TradeInfo()
                {
                    Direction = TradeDirection.Buy,
                    Code = code,
                    Amount = (int)(p_fundpergetin.Value / klineItemDay.CLOSE),
                    EntrustPrice = klineItemDay.CLOSE,
                    EntrustDate = d,
                    TradeDate = d,
                    TradePrice = klineItemDay.CLOSE,
                    Stamps = stampduty,
                    Fee = volumecommission,
                    TradeMethod = TradeInfo.TM_AUTO,
                    Reason = (p_mainforcelow <= 0 ? "" : "[主力线低位" + p_mainforcelow.ToString("F2")+"]") + (p_mainforceslope <= 0 ? "" : "[主力线上升速度超过" + p_mainforceslope.ToString("F2")+"]")
                };
                results.Add(tradeInfo);
            }

            return results;
        }

        public override TradeRecords Execute(string code, Properties strategyParam, BacktestParameter backtestParam, ISeller seller = null)
        {
            throw new NotImplementedException();
        }
    }
}
