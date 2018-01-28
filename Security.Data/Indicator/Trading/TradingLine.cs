using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Bean;
using insp.Utility.Collections.Time;

using insp.Security.Data.kline;
using insp.Utility.Date;

namespace insp.Security.Data.Indicator
{
    /// <summary>
    /// 买卖线
    /// </summary>
    public class TradingLine : IIndicator
    {
        public const char BUY = 'B';
        public const char SELL = 'S';
        
        /// <summary>
        /// 买线
        /// </summary>
        public TimeSeries<ITimeSeriesItem<double>> buyLine;
        /// <summary>
        /// 卖线
        /// </summary>
        public TimeSeries<ITimeSeriesItem<double>> sellLine;
        /// <summary>
        /// 买卖点
        /// </summary>
        public TimeSeries<ITimeSeriesItem<char>> buysellPoints;
    }
    /// <summary>
    /// 立体买卖计算工具
    /// </summary>
    public static class TradingLineUtils
    {
        

        /// <summary>
        /// 买线:EMA(CLOSE,3),COLORRED,LINETHICK1;
        /// 卖线:EMA(SLOPE(CLOSE,21)*20+CLOSE,42),COLORBLUE,LINETHICK2;
        /// </summary>
        /// <param name="kline"></param>
        /// <returns></returns>
        public static TradingLine indicator_trading_stereo1(this KLine kline,int begin=0,int end=0)
        {
            TimeSeries<ITimeSeriesItem<double>> close = kline.Select<double>("close",begin,end);

            TimeSeries<ITimeSeriesItem<double>> buyLine = close.EMA(3);
            TimeSeries<ITimeSeriesItem<double>> sellLine = (close.SLOPE(21)*20 + close).EMA(42);
            TimeSeries<ITimeSeriesItem<char>> buysellPoints = new TimeSeries<ITimeSeriesItem<char>>();

            int buy_gt_sell = 1, sell_gt_buy = 2;//买大于卖为1，卖大于买为2
            int state = 0;  //前一个状态
            for(int i=0;i<buyLine.Count;i++)
            {
                ITimeSeriesItem<double> buyItem = buyLine[i];
                ITimeSeriesItem<double> sellItem = sellLine[buyItem.Date];
                if (sellItem == null) continue;
                double t = buyItem.Value - sellItem.Value;
                if (t == 0) continue;
                if (state == 0)
                {
                    state = t > 0 ? buy_gt_sell : sell_gt_buy;
                    continue;
                }
                int cs = t > 0 ? buy_gt_sell : sell_gt_buy;
                if(cs == state)
                {
                    continue;
                }

                if(state == buy_gt_sell)
                {
                    TimeSeriesItem<char> v = new TimeSeriesItem<char>()
                    {
                        Date = buyItem.Date,
                        Value = 'S'
                    };
                    buysellPoints.Add(v);
                }
                else if(state == sell_gt_buy)
                {
                    TimeSeriesItem<char> v = new TimeSeriesItem<char>()
                    {
                        Date = buyItem.Date,
                        Value = 'B'
                    };
                    buysellPoints.Add(v);
                }
                state = cs;
            }

            return new TradingLine()
            {
                buyLine = buyLine,
                sellLine = sellLine,
                buysellPoints = buysellPoints
            };
        }
    }
}
