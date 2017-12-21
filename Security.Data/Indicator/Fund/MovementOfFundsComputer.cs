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
    public static class MovementOfFundsComputer
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
        public static TimeSeries<ITimeSeriesItem<List<double>>> executeIndicator(this KLine kline, int begin = 0, int end = 0, PropertyDescriptorCollection param = null)
        {
            TimeSeries<ITimeSeriesItem<double>> close = kline.Select<double>("CLOSE", begin, end);
            TimeSeries<ITimeSeriesItem<double>> open = kline.Select<double>("OPEN", begin, end);
            TimeSeries<ITimeSeriesItem<double>> high = kline.Select<double>("HIGH", begin, end);
            TimeSeries<ITimeSeriesItem<double>> low = kline.Select<double>("LOW", begin, end);
            TimeSeries<ITimeSeriesItem<double>> VAR0 = (close * 2.0 + high + low) / 4;


            TimeSeries<ITimeSeriesItem<double>> t1 = VAR0 - low.LLV(20);
            TimeSeries<ITimeSeriesItem<double>> t2 = high.HHV(20) - low.LLV(20);

            TimeSeries<ITimeSeriesItem<double>> t3 = (t1 / t2) * 100;

            //TimeSeries<ITimeSeriesItem<double>> B = t3.XMA(12);
            TimeSeries<ITimeSeriesItem<double>> B = t3.EMA(12);

            TimeSeries<ITimeSeriesItem<double>> mainforces = B.EMA(3);

            TimeSeries<ITimeSeriesItem<double>> retailInverstors = mainforces.EMA(30);


            TimeSeries<ITimeSeriesItem<List<double>>> results = new TimeSeries<ITimeSeriesItem<List<double>>>();
            foreach(ITimeSeriesItem<double> mainforce in mainforces)
            {
                TimeSeriesItem<List<double>> r = new TimeSeriesItem<List<double>>();
                r.Value = new List<double>(new double[2] { 0,0});

                r.Date = mainforce.Date;
                r.Value[0] = mainforce.Value;

                ITimeSeriesItem<double> retailInverstor = retailInverstors[r.Date];
                r.Value[1] = retailInverstor == null ? 0 : retailInverstor.Value;

                results.Add(r);
            }
            return results;
        }
    }
}
