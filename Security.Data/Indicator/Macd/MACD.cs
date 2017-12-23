using insp.Utility.Collections.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Security.Data.kline;

namespace insp.Security.Data.Indicator.Macd
{
    public class MACD : TimeSeries<MACDItem>, IIndicator
    {
        public readonly static IndicatorMeta Meta;

        static MACD()
        {
            new MACDItem();


            Meta = new IndicatorMeta("macd", "MACD", typeof(MACD), IndicatorClass.TimeSeries, null);

        }
        public MACD(String code, TimeUnit tu):base(code,tu)
        {

        }
        /// <summary>
        /// DIF:EMA(CLOSE,SHORT)-EMA(CLOSE,LONG);
        /// DEA:EMA(DIF, MID);
        /// MACD:(DIF-DEA)*2,COLORSTICK;
        /// 其中SHORT = 12,LONG = 26,MID = 9
        /// </summary>
        /// <param name="kline"></param>
        /// <returns></returns>
        public static MACD Create(KLine kline)
        {
            if (kline == null)
                return null;
            int p_short = 12, p_long = 26, p_mid = 9;

            TimeSeries<ITimeSeriesItem<double>> CLOSE = kline.Select<double>("close");
            TimeSeries<ITimeSeriesItem<double>> DIF = CLOSE.EMA(p_short) - CLOSE.EMA(p_long);
            TimeSeries<ITimeSeriesItem<double>> DEA = DIF.EMA(p_mid);
            TimeSeries<ITimeSeriesItem<double>> MACD = (DIF - DEA) * 2;

            MACD macd = new MACD(kline.Code, kline.TimeUnit);

            double prevDif = 0, prevdea = 0;
            for(int i=0;i<kline.Count;i++)
            {
                DateTime d = kline[i].Date;
                MACDItem item = new MACDItem();
                item.Date = d;

                
                ITimeSeriesItem<double> difItem = DIF[d];
                if (difItem == null) continue;
                item.DIF = difItem.Value;
                if (prevDif == 0) prevDif = difItem.Value;

                ITimeSeriesItem<double> deaItem = DEA[d];
                if (deaItem == null) continue;
                item.DEA = deaItem.Value;
                if (prevdea == 0) prevdea = deaItem.Value;

                ITimeSeriesItem<double> macdItem = MACD[d];
                if (macdItem == null) continue;
                item.MACD = macdItem.Value;

                if (prevDif < prevdea && difItem.Value > deaItem.Value)
                    item.CROSS = difItem.Value - deaItem.Value;//金叉点
                else if(prevDif > prevdea && difItem.Value < deaItem.Value)
                    item.CROSS = difItem.Value - deaItem.Value;//死叉点
                macd.Add(item);
            }

            return macd;
        }
    }
}
