using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;
using insp.Utility.Collections;
using insp.Utility.Collections.Tree;
using insp.Utility.Collections.Time;

using insp.Security.Data;
using insp.Security.Data.kline;
using insp.Security.Data.Indicator;

using insp.Utility.IO;
using insp.Utility.Bean;
using insp.Utility.Common;

using insp.Security.Strategy;

namespace Security.Alpha4.Test
{
    public class TestCase2 : TestCaseBase
    {
        int maxdays = 20;
        double maxProfilt = 0.06;

        public void Execute()
        {
            List<String> codes = new List<string>();
            System.IO.File.ReadAllLines(FileUtils.GetDirectory() + "test.csv")
                .ToList().ForEach(x => codes.Add(x.Split(',')[1]));


            IndicatorRepository repository = new IndicatorRepository("d:\\repository\\");
            repository.Initilization();


            foreach (String code in codes)
            {
                //生成数据
                TimeSerialsDataSet ds = repository[code];
                KLine dayLine = ds.DayKLine;
                KLine weekLine = dayLine.CreateWeek();
                ds.WeekKLine = weekLine;

                TimeSeries<ITimeSeriesItem<double>> dayClose = dayLine.Select<double>("close", 0, 0);
                TimeSeries<ITimeSeriesItem<double>> weekClose = weekLine.Select<double>("close", 0, 0);

                TradingLine dayTradeLine = ds.CubeCreateOrLoad(TimeUnit.day);
                TradingLine weekTradeLine = ds.CubeCreateOrLoad(TimeUnit.week);

                TimeSeries<ITimeSeriesItem<List<double>>> dayFunds = ds.FundTrendCreate(TimeUnit.day);
                TimeSeries<ITimeSeriesItem<List<double>>> weekFunds = ds.FundTrendCreate(TimeUnit.week);

                TimeSeries<ITimeSeriesItem<double>> dayCross = ds.FundTrendCrossCreateOrLoad(TimeUnit.day);
                TimeSeries<ITimeSeriesItem<double>> weedCross = ds.FundTrendCrossCreateOrLoad(TimeUnit.week);

                //测试买入

                List<TradeBout> bouts = new List<TradeBout>();
                TimeSeries<ITimeSeriesItem<char>> dayTradePt = dayTradeLine.buysellPoints;
                for (int i = 0; i < dayTradePt.Count; i++)
                {
                    ITimeSeriesItem<char> item = dayTradePt[i];
                    if (item.Value == 'S') continue;
                    if (item.Date < begin || item.Date > end) continue;
                    DateTime buyPtDate = item.Date;
                    int index = dayFunds.IndexOf(buyPtDate);
                    while (index <= dayFunds.Count)
                    {
                        ITimeSeriesItem<List<double>> fundItem = dayFunds[index];
                        if (fundItem == null)
                        {
                            index += 1;
                            continue;
                        }
                        if (fundItem.Value[0] <= fundItem.Value[1])
                        {
                            index += 1;
                            continue;
                        }
                        TradeBout bout = new TradeBout(code);
                        KLineItem klineItem = dayLine.GetNearest(fundItem.Date, false);
                        if (klineItem == null)
                        {
                            index += 1;
                            continue;
                        }
                        bout.RecordTrade(1, klineItem.Date, TradeDirection.Buy, klineItem.CLOSE, (int)(funds / klineItem.CLOSE), 0, 0, "发出B点且主力=" + fundItem.Value[0].ToString("F3") + "大于散户" + fundItem.Value[1].ToString("F3") + ",日期=" + fundItem.Date.ToString("yyyyMMdd"));
                        bouts.Add(bout);
                        break;
                    }
                }
                //测试卖出
                for (int i = 0; i < bouts.Count; i++)
                {
                    DateTime buyDate = bouts[i].BuyInfo.TradeDate;
                    int buyIndex = dayLine.IndexOf(buyDate);
                    int index = buyIndex + 1;
                    while (index <= dayLine.Count - 1)
                    {
                        KLineItem item = dayLine[index];
                        if (index - buyIndex >= maxdays)
                        {
                            bouts[i].RecordTrade(2, item.Date, TradeDirection.Sell, item.CLOSE, bouts[i].BuyInfo.Amount, 0, 0, "大于" + maxdays.ToString() + "天卖出");
                            break;
                        }
                        else
                        {
                            double profile = (item.HIGH - bouts[i].BuyInfo.TradePrice) / bouts[i].BuyInfo.TradePrice;
                            if (profile >= maxProfilt)
                            {
                                bouts[i].RecordTrade(2, item.Date, TradeDirection.Sell, (bouts[i].BuyInfo.TradePrice * (1 + maxProfilt)), bouts[i].BuyInfo.Amount, 0, 0, "利润大于" + maxdays.ToString() + "天卖出");
                                break;
                            }
                        }
                        index += 1;
                    }

                }
                //去掉未完成的
                for (int i = 0; i < bouts.Count; i++)
                {
                    if (!bouts[i].Completed)
                    {
                        bouts.RemoveAt(i--);
                    }
                }

                TradeRecords tradeRecords = new TradeRecords();
                tradeRecords.Bouts.AddRange(bouts);
                //打印结果
                for (int i = 0; i < bouts.Count; i++)
                {
                    Console.WriteLine(bouts[i].ToString());
                }
                Console.WriteLine(tradeRecords.ToString());

            }
        }
    }
}
