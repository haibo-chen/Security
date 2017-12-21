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
    public static class RetailInvestor
    {
        /// <summary>
        /// VAR0:=(2*CLOSE+HIGH+LOW)/4;
        ///B:=XMA((VAR0-LLV(LOW,30))/(HHV(HIGH,30)-LLV(LOW,30))*100,12); 
        ///主力做多资金:EMA(B,3),LINETHICK2,COLORWHITE; 
        ///个股做空资金:EMA(主力做多资金,30),COLORD9D919,LINETHICK2;
        /// </summary>
        /// <param name="kline"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static TimeSeries<ITimeSeriesItem<double>> indicator_fund_ratailinvestor1(this KLine kline, int begin=0, int end=0, PropertyDescriptorCollection param = null)
        {
            TimeSeries<ITimeSeriesItem<double>> t1 = kline.indicator_fund_main1(begin, end, param);
            return t1.EMA(30);
        }
    }
}
