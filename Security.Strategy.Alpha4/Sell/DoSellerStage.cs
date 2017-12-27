using insp.Security.Data;
using insp.Security.Data.kline;
using insp.Utility.Bean;
using insp.Utility.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Strategy.Alpha.Sell
{
    /// <summary>
    /// 阶梯方式卖出
    /// </summary>
    public class DoSellerStage : Seller
    {
        public override void Execute(List<TradeBout> bouts,Properties strategyParam,BacktestParameter backtestParam)
        {
            if (bouts == null || bouts.Count <= 0)
                return;
            IndicatorRepository repository = (IndicatorRepository)backtestParam.Get<Object>("repository");
            if (repository == null)
                return;
            //取得卖出策略参数
            int sell_maxholddays = strategyParam.Get<int>("sell_maxholddays");//最大持仓天数
            int sell_selectdays = strategyParam.Get<int>("sell_selectdays");//择机卖出最大天数
            double sell_stoploss = strategyParam.Get<double>("sell_stoploss");//止损线
            String sell_sellpt = strategyParam.Get<String>("sell_sellpt");//卖点
            List<double[]> sell_pts = StringUtils.ToDoubleArrayList(sell_sellpt,"|",";");
            if (sell_pts == null || sell_pts.Count<=0)
                return;

            foreach(TradeBout bout in bouts)
            {
                TimeSerialsDataSet sds = repository[bout.Code];
                if (sds == null) continue;

                DateTime buyDate = bout.BuyInfo.TradeDate;//买入日期                    
                KLine dayLine = sds.DayKLine;
                if (dayLine == null) continue;
                int buyDayLineIndex = dayLine.IndexOf(buyDate);
                if (buyDayLineIndex < 0) continue;

                int days = 0; //相对于买入日期的距离天数
                DateTime d;
                int curSelChangeDays = 0; //择机卖出状态的天数
                String curSelChangeReanson = "";
                for (int j = buyDayLineIndex + 1; j < dayLine.Count; j++)
                {
                    KLineItem item = dayLine[j];
                    d = item.Date;
                    days += 1;

                    //计算以d日收盘价卖出的盈利情况
                    bout.RecordTrade(2, d, TradeDirection.Sell, item.CLOSE, bout.BuyInfo.Amount, backtestParam.Volumecommission, backtestParam.Stampduty);
                    double earnCloseRate = bout.EarningsRate;
                    bout.TradeInfos[1] = null;
                    bout.RecordTrade(2, d, TradeDirection.Sell, item.HIGH, bout.BuyInfo.Amount, backtestParam.Volumecommission, backtestParam.Stampduty);
                    double earnHighRate = bout.EarningsRate;
                    bout.TradeInfos[1] = null;

                    //达到止损线
                    if(earnCloseRate * -1 >= sell_stoploss)
                    {
                        bout.RecordTrade(2, d, TradeDirection.Sell, item.CLOSE, bout.BuyInfo.Amount, backtestParam.Volumecommission, backtestParam.Stampduty, "达到止损线"+ earnCloseRate.ToString("F2"));
                        break;
                    }
                    //当前处于择机卖出状态
                    if (curSelChangeDays > 0)
                    {
                        if (earnCloseRate > 0) //不亏损就卖出
                        {
                            bout.RecordTrade(2, d, TradeDirection.Sell, item.CLOSE, bout.BuyInfo.Amount, backtestParam.Volumecommission, backtestParam.Stampduty, curSelChangeReanson + ",择机天数=" + curSelChangeDays.ToString());
                            break;
                        }
                        if (curSelChangeDays > sell_selectdays)//大于择机卖出最大天数
                        {
                            bout.RecordTrade(2, d, TradeDirection.Sell, item.CLOSE, bout.BuyInfo.Amount, backtestParam.Volumecommission, backtestParam.Stampduty, curSelChangeReanson + ",择机天数=" + curSelChangeDays.ToString());
                            break;
                        }
                        curSelChangeDays += 1;
                        continue;
                    }

                    //超过最大持仓日期
                    if (days >= sell_maxholddays)
                    {
                        curSelChangeReanson = "超过最大持仓日期";
                        curSelChangeDays = 1;
                        continue;
                    }

                    

                    //根据天数找到对应的策略卖点日期
                    int k = 0;
                    for (k = 0;k<sell_pts.Count-1;k++)
                    {
                        if (days > sell_pts[k][0])
                            continue;
                        break;
                    }
                    if(k >= sell_pts.Count)//没有找到合适卖点，准备择机卖出
                    {
                        curSelChangeReanson = "没有找到对应卖点";
                        curSelChangeDays = 1;
                        continue;
                    }
                    double expectProfile = sell_pts[k][1];
                    if(earnHighRate < expectProfile)//没有到达期望盈利
                    {
                        continue;
                    }
                    //按照预期盈利卖出
                    double price = bout.BuyInfo.TradePrice * (1.0 + expectProfile);
                    bout.RecordTrade(2, d, TradeDirection.Sell, price, bout.BuyInfo.Amount, backtestParam.Volumecommission, backtestParam.Stampduty, "盈利在第" + days.ToString() + "天时超过预期" + sell_pts[k][0].ToString() + ";" + expectProfile.ToString("F2"));
                    break;

                }
            }

        }

        class Temp
        {
            public String code;
            public String buyDate;
            public List<double> closeEarns = new List<double>();
            public List<double> highearns = new List<double>();

            public string ToString(int type)
            {
                if(type == 1)
                    return code + "," + buyDate + "," + (closeEarns.Count<=0?"":closeEarns.ConvertAll(x=>x.ToString("F3")).Aggregate((x,y)=>x+","+y));
                else
                    return code + "," + buyDate + "," + (highearns.Count <= 0 ? "" : highearns.ConvertAll(x => x.ToString("F3")).Aggregate((x, y) => x + "," + y));
            }
        }

        List<Temp> temps = new List<Temp>();

        public void test(List<TradeBout> bouts, Properties strategyParam, BacktestParameter backtestParam)
        {
            temps.Clear();
            if (bouts == null || bouts.Count <= 0)
                return;
            IndicatorRepository repository = (IndicatorRepository)backtestParam.Get<Object>("repository");
            if (repository == null)
                return;
            //取得卖出策略参数
            int sell_maxholddays = strategyParam.Get<int>("sell_maxholddays");//最大持仓天数
            int sell_selectdays = strategyParam.Get<int>("sell_selectdays");//择机卖出最大天数
            double sell_stoploss = strategyParam.Get<double>("sell_stoploss");//止损线
            String sell_sellpt = strategyParam.Get<String>("sell_sellpt");//卖点
            List<double[]> sell_pts = StringUtils.ToDoubleArrayList(sell_sellpt, "|", ";");
            if (sell_pts == null || sell_pts.Count <= 0)
                return;

            foreach (TradeBout bout in bouts)
            {
                TimeSerialsDataSet sds = repository[bout.Code];
                if (sds == null) continue;

                

                DateTime buyDate = bout.BuyInfo.TradeDate;//买入日期                    
                KLine dayLine = sds.DayKLine;
                if (dayLine == null) continue;
                int buyDayLineIndex = dayLine.IndexOf(buyDate);
                if (buyDayLineIndex < 0) continue;

                int days = 0; //相对于买入日期的距离天数
                DateTime d;

                Temp t = new Temp();
                t.code = sds.Code;
                t.buyDate = buyDate.ToString("yyyyMMdd");
                temps.Add(t);
                for (int j = buyDayLineIndex + 1; j <= buyDayLineIndex + sell_maxholddays; j++)
                {
                    if (j >= dayLine.Count)
                        break;
                    KLineItem item = dayLine[j];
                    d = item.Date;
                    days += 1;

                    //计算以d日收盘价卖出的盈利情况
                    bout.RecordTrade(2, d, TradeDirection.Sell, item.CLOSE, bout.BuyInfo.Amount, backtestParam.Volumecommission, backtestParam.Stampduty);
                    double earnCloseRate = bout.EarningsRate;
                    bout.TradeInfos[1] = null;
                    bout.RecordTrade(2, d, TradeDirection.Sell, item.HIGH, bout.BuyInfo.Amount, backtestParam.Volumecommission, backtestParam.Stampduty);
                    double earnHighRate = bout.EarningsRate;
                    bout.TradeInfos[1] = null;

                   
                   
                    t.closeEarns.Add(earnCloseRate);
                    t.highearns.Add(earnHighRate);
                    
                    
                }
            }
            save();
        }

        private void save()
        {
            String filename = "d:\\temp.csv";
            if (temps == null || temps.Count <= 0) return;

            int num1 = temps.Min(x => x.closeEarns.Count);
            int num2 = temps.Min(x => x.highearns.Count);
            int num = Math.Min(num1,num2);

            StringBuilder str1 = new StringBuilder();
            StringBuilder str2 = new StringBuilder();
            for (int i=0;i<num;i++)
            {
                if (str1.ToString() != "")
                    str1.Append(",");
                str1.Append(temps.ConvertAll(x => x.closeEarns[i]).Average().ToString("F3"));
                if (str2.ToString() != "")
                    str2.Append(",");
                str1.Append(temps.ConvertAll(x => x.highearns[i]).Average().ToString("F3"));

            }
            System.IO.File.WriteAllText(filename,str1.ToString() + System.Environment.NewLine + str2.ToString());

            
            System.IO.File.WriteAllLines("d:\\temp1.csv",temps.ConvertAll(x=>x.ToString(1)).ToArray());
            System.IO.File.WriteAllLines("d:\\temp2.csv",temps.ConvertAll(x => x.ToString(2)).ToArray());
            
        }
    }
}
