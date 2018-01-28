using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using insp.Utility.Bean;
using insp.Security.Data;
using insp.Utility.Collections.Time;
using insp.Security.Data.kline;

namespace insp.Security.Strategy.Alpha.Sell
{
    public class DoBuyB : Buyer
    {
        public override List<TradeInfo> DoBuy(Properties strategyParams, DateTime d, StrategyContext context)
        {
            throw new NotImplementedException();
        }

        public override TradeRecords Execute(string code, Properties strategyParam, BacktestParameter backtestParam, ISeller seller = null)
        {
            IndicatorRepository repository = (IndicatorRepository)backtestParam.Get<Object>("repository");
            if (repository == null) return null;

            TimeSerialsDataSet ds = repository[code];
            if (ds == null) return null;

            TimeSeries<ITimeSeriesItem<char>> ts = ds.DayTradeLine.buysellPoints;
            if (ts == null) return null;

            TimeSeries<ITimeSeriesItem<List<double>>> fundTrends = ds.DayFundTrend;

            KLine kline = ds.DayKLine;

            TradeRecords tr = new TradeRecords(code);

            GetInMode getin = GetInMode.Parse(strategyParam.Get<String>("getinMode"));
            int diffdays = strategyParam.Get<int>("diffdays");

            for (int i=0;i<ts.Count;i++)
            {
                if (ts[i].Date.Date < backtestParam.BeginDate) continue;
                if (ts[i].Date.Date >= backtestParam.EndDate) continue;

                if (ts[i].Value == 'S')
                    continue;
                TradeBout bout = new TradeBout(code);

                //主力线大于散户线，且连续diffdays天与散户线拉大距离
                if (diffdays > 0 && fundTrends != null)
                {
                    int fi = fundTrends.IndexOf(ts[i].Date);
                    if (fi < 0 || fi< diffdays-1) continue;

                    ITimeSeriesItem<List<double>> ftItem = fundTrends[fi];
                    if (ftItem.Value[0] >= 30) continue;
                    double diff = ftItem.Value[0] - ftItem.Value[1];
                    if (diff <= 0) continue;

                    bool continuekuoda = true;
                    for (int t = 1;t<diffdays;t++)
                    {
                        ftItem = fundTrends[fi - i];
                        double tDiff = ftItem.Value[0] - ftItem.Value[1];
                        if(diff<tDiff)
                        {
                            continuekuoda = false;
                            break;
                        }
                        diff = tDiff;
                    }
                    if (!continuekuoda)
                        continue;


                }
                KLineItem item = kline[ts[i].Date];
                bout.RecordTrade(1, ts[i].Date, TradeDirection.Buy, item.CLOSE, (int)(getin.Value / item.CLOSE), backtestParam.Volumecommission, backtestParam.Stampduty, "B");
                tr.Bouts.Add(bout);
            }

            return tr;

        }
    }
}
