using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Bean;
using insp.Utility.Collections.Time;
using insp.Utility.Date;

namespace insp.Security.Data.kline
{
    public class KLine : TimeSeries<KLineItem>, IIndicator
    {
        public readonly static IndicatorMeta Meta;
        
        static KLine()
        {
            new KLineItem();


            Meta = new IndicatorMeta("kline", "kline", typeof(KLine), IndicatorClass.TimeSeries, null);

        }
        public KLine(String code,TimeUnit tu):base(code,tu)
        {

        }
        public override Func<List<KLineItem>, KLineItem> getElementMerger()
        {
            return doMerge;
        }
        public static KLineItem doMerge(List<KLineItem> list)
        {
            if (list == null || list.Count <= 0) return null;
            KLineItem item = new KLineItem();
            item.SetValue<String>("code", list[0].GetValue<String>("code"));
            item.SetValue<DateTime>("time", list[list.Count - 1].Date);
            item.SetValue<double>("open", list[0].OPEN);
            item.SetValue<double>("close", list[list.Count - 1].CLOSE);
            item.SetValue<double>("high", list.ConvertAll(x => x.HIGH).Max());
            item.SetValue<double>("low", list.ConvertAll(x => x.LOW).Min());
            item.SetValue<double>("volume", list.ConvertAll(x => x.GetValue<double>("volume")).Sum());
            item.SetValue<double>("turnover", list.ConvertAll(x => x.GetValue<double>("turnover")).Sum());
            return item;
        }
        /// <summary>
        /// 根据日线创建周线
        /// </summary>
        /// <returns></returns>
        public KLine CreateWeek()
        {
            DateTime begin = this[0].Date;
            DateTime end = this[this.Count - 1].Date;
            return CreateWeek(begin,end);
        }

        /// <summary>
        /// 根据日线创建周线
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public KLine CreateWeek(DateTime begin,DateTime end)
        {
            KLine weekKLine = new KLine(this.Code,TimeUnit.week);

            DateTime d = CalendarUtils.GetWeek(begin, DayOfWeek.Friday);
            while (d <= end)
            {
                KLineItem weekItem = new KLineItem();
                DateTime t = d;
                KLineItem t1 = this.GetNearest(t, false);
                if (t1 == null) { d = d.AddDays(7); continue; }
                weekItem.SetValue<double>("close", t1.CLOSE);
                t1 = this.GetNearest(t.AddDays(-4), true,0);
                if (t1 == null) { d = d.AddDays(7); continue; }
                weekItem.SetValue<double>("close",t1.OPEN);

                int index = this.IndexOf(t1);
                double high =0;
                double low = 0;
                while (index < this.Count && this[index].Date <= d)
                {
                    if (high==0 || high < this[index].HIGH)
                        high = this[index].HIGH;
                    if (low > this[index].LOW)
                        low = this[index].LOW;
                    index += 1;
                }
                if (low == 0 || high == 0)
                {
                    d = d.AddDays(7);
                    continue;
                }
                    
                weekItem.SetValue<double>("high",high);
                weekItem.SetValue<double>("low",low);
                weekItem.SetValue<DateTime>("time",d);
                weekItem.SetValue<String>("code",Code);
                weekKLine.Add(weekItem);

                d = d.AddDays(7);
            }
            return weekKLine;
            
        }

        /// <summary>
        /// 取得离d日期最近的数据
        /// </summary>
        /// <param name="d"></param>
        /// <param name="backward">向回找</param>
        /// <param name="rough"></param>
        /// <returns></returns>
        public KLineItem GetNearest(DateTime d,bool backward=true,int rough=10)
        {
            if (this.Count <= 0)
                return null;

            KLineItem item = this[d];
            if (item != null)
                return item;
            
            
            while(item == null && (rough==-1 || rough-- >0))
            {
                d = d.AddDays(backward?- 1:1);
                item = this[d];
                if (item != null)
                    return item;
            }
            return null;
        }
    }

    public static class KLineStatUtils
    {

    }
}
