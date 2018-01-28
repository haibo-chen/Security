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
    public class DoSellerComposite : Seller
    {
        public override bool DoSell(String code, TradeBout bout, DateTime d, Properties strategyParam, BacktestParameter backtestParam,out String reason)
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

            //取得策略参数
            int p_spoints = strategyParam.Get<int>("spoints");
            int p_totaldropcount = strategyParam.Get<int>("totaldropcount");
            int p_continuedropcount = strategyParam.Get<int>("continuedropcount");
            

            #region 判断发出S点卖出：p_spoints=1表示日线出S点，p_spoints=2表示60分钟线出S点
            if (p_spoints >0 && ds.DayTradeLine != null && ds.DayTradeLine.buysellPoints != null && ds.DayTradeLine.buysellPoints.Count>0)
            {
                ITimeSeriesItem<char> bsptItemDay = ds.DayTradeLine.buysellPoints[d];
                if(bsptItemDay != null && bsptItemDay.Value == 'S')
                {
                    KLineItem klineItemDay = kline[d];
                    if(klineItemDay != null)
                    {
                        double price = klineItemDay.CLOSE;
                        int amount = bout.BuyInfo.Amount;
                        bout.RecordTrade(2, klineItemDay.Date, TradeDirection.Sell, price, amount, backtestParam.Volumecommission, backtestParam.Stampduty, "日线出S点");
                        return false;
                    }                    
                }
            }
            #endregion

            #region 根据价格下降情况判断是否卖出
            if(p_continuedropcount > 0 || p_totaldropcount >0 )
            {
                int bIndex = kline.IndexOf(bout.BuyInfo.TradeDate);                
                
                int index = bIndex + 1;
                if (index >= kline.Count)
                    return false;
                KLineItem item = kline[index];
                DateTime td = item.Date;

                int totaldropcount = 0,continuedropcount=0;
                double prevPrice = bout.BuyInfo.TradePrice;
                bool prevDrop = item.Average < prevPrice;
                while(td<=d && index < kline.Count-1)
                {
                    double price = item.Average;
                    if(price < prevPrice)
                    {
                        totaldropcount += 1;
                        if (prevDrop)
                            continuedropcount += 1;
                        prevDrop = true;
                    }
                    else
                    {
                        prevDrop = false;
                    }

                    index += 1;
                    item = kline[index];
                    td = item.Date;
                }

                //进入到伺机卖出状态
                if(p_continuedropcount != 0 && continuedropcount>= p_continuedropcount)
                {
                    return true;
                }
                //进入到伺机卖出情况
                if(p_totaldropcount != 0 && totaldropcount >= p_totaldropcount)
                {
                    return true;
                }
                
            }
            #endregion


            return false;

        }

        public override TradeInfo DoSell(HoldRecord holdRecord, DateTime d, Properties strategyParams, StrategyContext context)
        {
            throw new NotImplementedException();
        }
    }
}
