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
    /// <summary>
    /// 查找最好的买点
    /// </summary>
    public class DoBuyBest : Buyer
    {
        public override List<TradeInfo> DoBuy(Properties strategyParams, DateTime d, StrategyContext context)
        {
            throw new NotImplementedException();
        }

        public override TradeRecords Execute(string code, Properties strategyParam, BacktestParameter backtestParam, ISeller seller = null)
        {
            //获取行情数据
            IndicatorRepository repository = (IndicatorRepository)backtestParam.Get<Object>("repository");
            if (repository == null) return null;

            TimeSerialsDataSet ds = repository[code];
            if (ds == null) return null;

            KLine kline = ds.DayKLine;
            if (kline == null) return null;

            DateTime beginDate = backtestParam.BeginDate;
            DateTime endDate = backtestParam.EndDate;

            int bIndex = kline.IndexOf(beginDate, false);
            if (bIndex < 0) return null;

            //获取参数
            int maxholddays = strategyParam.Get<int>("maxholddays");
            double maxprofilt = strategyParam.Get<double>("maxprofilt");

            //遍历K线      
            List<Object[]> listEarn = new List<object[]>();
            List<Object[]> listLoss = new List<object[]>();
            TradeRecords tr = new TradeRecords();
            for (int index = bIndex;index < kline.Count;index++)
            {
                KLineItem item = kline[index];
                if (item.Date >= endDate) break;

                

                double buyPrice = item.CLOSE;
                double maxProfiltEffenicePerStock = double.MinValue + 1;
                double maxLossEffenicePerStock = double.MaxValue - 1;
                int maxProfiltHoldDays = 0;
                int maxLossHoldDays = 0;
                KLineItem sellEarnItem = null;
                KLineItem sellLossItem = null;
                for (int i=1;i< maxholddays;i++)
                {
                    if (index + i >= kline.Count)
                        break;
                    
                    KLineItem item2 = kline[index+i];
                    if ((item2.Date - item.Date).TotalDays > maxholddays)
                        break;

                    double eraningRates = (item2.CLOSE - buyPrice)/ buyPrice;

                    if(eraningRates > 0 && eraningRates >= maxprofilt && maxProfiltEffenicePerStock < eraningRates)
                    {
                        maxProfiltEffenicePerStock = eraningRates/i;
                        maxProfiltHoldDays = i;
                        sellEarnItem = item2;
                    }  
                    if(eraningRates < 0 && maxLossEffenicePerStock > eraningRates)
                    {
                        maxLossEffenicePerStock = eraningRates;
                        maxLossHoldDays = i;
                        sellLossItem = item2;
                    }
                }

                if(maxProfiltHoldDays > 0)
                {
                    Object[] objs = new Object[] { item, sellEarnItem, maxProfiltEffenicePerStock,(sellEarnItem.CLOSE- buyPrice)/ buyPrice };
                    listEarn.Add(objs);
                    index = kline.IndexOf(sellEarnItem);
                }
                if(maxLossHoldDays > 0)
                {
                    Object[] objs = new Object[] { item, sellLossItem, maxLossEffenicePerStock, (sellLossItem.CLOSE - buyPrice) / buyPrice };
                    listLoss.Add(objs);
                }
            }

            if (listEarn.Count <= 0)
                return null;
            Comparison<Object[]> comparsionEran = (x, y) =>
            {
                return (int)((double)y[2] - (double)x[2]);
            };
            listEarn.Sort(comparsionEran);

            List<String> strs = new List<string>();
            foreach(Object[] objs in listEarn)
            {
                KLineItem buyItem = (KLineItem)objs[0];
                KLineItem sellItem = (KLineItem)objs[1];

                String str = code + "," + ((double)objs[2]).ToString("F3") + "," + ((double)objs[3]).ToString("F3") + "," + (sellItem.Date - buyItem.Date).TotalDays.ToString() + "," +
                                     buyItem.Date.ToString("yyyyMMdd") + "," + sellItem.Date.ToString("yyyyMMdd") +","+
                                     buyItem.CLOSE.ToString("F2") + "," + sellItem.CLOSE.ToString("F2");
                strs.Add(str);
                logger.Info(str);
            }

            System.IO.File.AppendAllLines(backtestParam.Resultpath + "temp.csv", strs.ToArray(), Encoding.UTF8);
            return null;



        }
    }
}
