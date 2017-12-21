using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Bean;
using insp.Utility.Collections.Time;

using insp.Security.Data.kline;


namespace insp.Security.Data.Indicator
{
    public static class MainForce
    {
        /// <summary>
        /// VAR0:=(2*CLOSE+HIGH+LOW)/4;
        /// B:=XMA((VAR0-LLV(LOW,30))/(HHV(HIGH,30)-LLV(LOW,30))*100,12); 
        /// 主力做多资金:EMA(B,3),LINETHICK2,COLORWHITE;
        /// </summary>
        /// <param name="kline"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static TimeSeries<ITimeSeriesItem<double>> indicator_fund_main1(this KLine kline,int begin=0,int end=0,PropertyDescriptorCollection param=null)
        {
            TimeSeries<ITimeSeriesItem<double>> close = kline.Select<double>("CLOSE", begin, end);
            TimeSeries<ITimeSeriesItem<double>> open = kline.Select<double>("OPEN", begin, end);
            TimeSeries<ITimeSeriesItem<double>> high = kline.Select<double>("HIGH", begin, end);
            TimeSeries<ITimeSeriesItem<double>> low = kline.Select<double>("LOW", begin, end);
            TimeSeries<ITimeSeriesItem<double>> VAR0 = (close * 2.0 + open + low) / 4;

            
            TimeSeries<ITimeSeriesItem<double>> t1 = VAR0 - low.LLV(30);
            TimeSeries<ITimeSeriesItem<double>> t2 = high.HHV(30) - low.LLV(30);

            TimeSeries<ITimeSeriesItem<double>> t3 = (t1 / t2) * 100;

            TimeSeries<ITimeSeriesItem<double>> B = t3.XMA(12);

            TimeSeries<ITimeSeriesItem<double>> results = B.EMA(3);
            return results;

        }
    }
}
