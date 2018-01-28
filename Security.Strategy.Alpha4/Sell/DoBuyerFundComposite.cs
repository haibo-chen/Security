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
    
    public class DoBuyerFundComposite : Buyer
    {
        public override List<TradeInfo> DoBuy(Properties strategyParams, DateTime d, StrategyContext context)
        {
            throw new NotImplementedException();
        }

        public override TradeRecords Execute(string code, Properties strategyParam, BacktestParameter backtestParam, ISeller seller = null)
        {
            IndicatorRepository repository = (IndicatorRepository)backtestParam.Get<Object>("repository");
            if (repository == null) return null;

            //创建数据集
            TimeSerialsDataSet ds = repository[code];
            if (ds == null)
                return null;
            KLine klineDay = ds.DayKLine;
            if (klineDay == null || klineDay.Count < 0)
                return null;
            TimeSeries<ITimeSeriesItem<List<double>>> fundDay = ds.DayFundTrend;
            if (fundDay == null || fundDay.Count <= 0)
                return null;

            double p_mainforcelow = strategyParam.Get<double>("mainforcelow");
            int p_monthbutpt = strategyParam.Get<int>("monthbutpt",0);
            //double p_mainforceclimb = strategyParam.Get<double>("mainforceclimb");
            double p_mainforceslope = strategyParam.Get<double>("mainforceslope");
            int p_mainforcerough = strategyParam.Get<int>("mainforcerough");
            int p_buypointdays = strategyParam.Get<int>("buypointdays");
            int p_maxbuynum = strategyParam.Get<int>("maxbuynum");
            GetInMode p_fundpergetin = GetInMode.Parse(strategyParam.Get<String>("getinMode"));

            TradeRecords tradeRecords = new TradeRecords(code);

            //遍历回测中的每一天
            DateTime d = backtestParam.BeginDate;
            int beginIndex = klineDay.IndexOf(d,true);
            if (beginIndex < 0) return tradeRecords;
            for (int index = beginIndex;index<klineDay.Count;index++)
            {
                KLineItem klineItemDay = klineDay[index];
                if (klineItemDay == null) continue;
                d = klineItemDay.Date;

                ITimeSeriesItem<List<double>> fundItemDay = fundDay[d];
                if (fundItemDay == null) continue;
                int fIndex = fundDay.IndexOf(fundItemDay);

                
                //是否进入到主力线低位
                if (p_mainforcelow != 0 && fundItemDay.Value[0] >= p_mainforcelow)
                    continue;

                //是否主力线爬升离开低位
                if(p_mainforcelow != 0)
                {                    
                    for (fIndex = fIndex + 1; fIndex < fundDay.Count; fIndex++)
                    {                       
                        fundItemDay = fundDay[fIndex];
                        if (fundItemDay == null) continue;

                        if (fundItemDay.Value[0] <= p_mainforcelow)
                            continue;

                        if (fundItemDay.Date < backtestParam.BeginDate || fundItemDay.Date > backtestParam.EndDate)//数据错误
                            return tradeRecords;

                        d = fundItemDay.Date;
                        index = klineDay.IndexOf(d);
                        klineItemDay = klineDay[index];
                        break;
                    }
                    if (fIndex >= fundDay.Count)
                        return tradeRecords;
                }

                //看主力线爬升速度
                if(p_mainforceslope != 0 && fIndex>0)
                {
                    //爬升速度不够快
                    if((fundItemDay.Value[0] - fundDay[fIndex-1].Value[0]) < p_mainforceslope)
                    {
                        continue;
                    }
                }

                //看主力线是否持续爬升
                if(p_mainforcerough > 0)
                {
                    bool cont = true;
                    for(int temp = 0;temp< p_mainforcerough;temp++)
                    {
                        fIndex += temp;
                        if(fIndex >= fundDay.Count)
                        {
                            cont = false;
                            break;
                        }
                        fundItemDay = fundDay[fIndex];

                        if (fundItemDay.Value[0] < fundDay[fIndex - 1].Value[0])
                        {
                            cont = false;
                            break;
                        }
                    }
                    if (!cont)
                        continue;
                    
                    d = fundItemDay.Date;
                    index = klineDay.IndexOf(d);
                    klineItemDay = klineDay[index];
                }

                //看是否在买点附近
                TradingLine tradingLine = ds.DayTradeLine;
                if (p_buypointdays >= 0 && tradingLine != null && tradingLine.buysellPoints != null && tradingLine.buysellPoints.Count>0)
                {
                    int bsptIndex = tradingLine.buysellPoints.IndexOf(d, true);
                    ITimeSeriesItem<char> bsptItemDay = bsptIndex < 0 ? null : tradingLine.buysellPoints[bsptIndex];
                    if (bsptItemDay != null && bsptItemDay.Value == 'S')
                        bsptItemDay = bsptIndex >= tradingLine.buysellPoints.Count - 1 ? null : tradingLine.buysellPoints[bsptIndex + 1];
                    if (bsptItemDay == null || (bsptItemDay.Date.Date - d).TotalDays > p_buypointdays)
                        continue;
                }

                //月线买点才能买入
                TimeSeries<ITimeSeriesItem<char>> ptMonths = ds.CubePtCreateOrLoad(TimeUnit.month);
                if (p_monthbutpt == 1 && ptMonths != null && ptMonths.Count>0)
                {
                    int t1 = 0;
                    for (;t1<ptMonths.Count-1;t1++)
                    {
                        if (d.Date >= ptMonths[t1].Date.Date && d.Date <= ptMonths[t1 + 1].Date.Date)
                            break;
                    }
                    if(t1 < ptMonths.Count - 1)
                    {
                        if (ptMonths[t1].Value != 'B')
                            continue;
                    }

                }
                //准备执行买入
                String reason = "";
                double price = klineItemDay.CLOSE;
                double fund = p_fundpergetin.Value;// price * p_maxholdnum;

                int amount = (int)(fund / price);
                TradeBout newBout = new TradeBout(ds.Code);
                newBout.RecordTrade(1, d, TradeDirection.Buy, price, amount, backtestParam.Volumecommission, 0, reason);               
                tradeRecords.Bouts.Add(newBout);
                
            }
            return tradeRecords;
        }
    }
}
