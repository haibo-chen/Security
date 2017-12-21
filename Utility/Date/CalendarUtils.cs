using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using insp.Utility.Collections.Time;

namespace insp.Utility.Date
{
    public enum WorkDayType
    {
        Rest,Work
    }

    public enum SpecialDayClass
    {
        /// <summary>周末</summary>         
        Weekend = 1,
        /// <summary>节日</summary>         
        Festival = 2,
        /// <summary>假日</summary>         
        Hoilday = 4,
        /// <summary>调整工作日</summary>         
        AdjustingWorkday = 8
    }
    /// <summary>
    /// 特殊日期
    /// </summary>
    [XmlRoot]
    public class SpecialDay
    {
        private DateTime date;
        
        /// <summary>
        /// 日期
        /// </summary>
        [XmlAttribute]
        public String Date
        {
            get { return date.ToString("yyyy-MM-dd"); }
            set { date = DateUtils.Parse(value,"yyyy-MM-dd"); }
        }
        /// <summary>
        /// 日期
        /// </summary>
        [XmlIgnore]
        public DateTime DateValue
        {
            get { return date; }
        }
        /// <summary>
        /// 工作日类型
        /// </summary>
        [XmlAttribute]
        public WorkDayType DayType;
        /// <summary>
        /// 特殊类别
        /// </summary>
        [XmlAttribute]
        public SpecialDayClass SpecialClass;

        public override string ToString()
        {
            return Date + ",DayType" + DayType.ToString();
        }

    }

    [XmlRoot(ElementName = "configuration")]
    public class SpecialDays
    {
        [XmlArray(ElementName = "items")]
        [XmlArrayItem(ElementName = "item", Type = typeof(SpecialDay))]
        public List<SpecialDay> Items = new List<SpecialDay>();
    }

    /// <summary>
    /// 日历工具
    /// </summary>
    public class CalendarUtils
    {
        #region 工作日管理
        /// <summary>
        /// 特定日期
        /// </summary>
        private static SpecialDays specialDays;
        /// <summary>
        /// 特定日期 
        /// </summary>
        public static SpecialDays SpecialDays { get { return specialDays; } }
        static CalendarUtils()
        {
            specialDays = insp.Utility.IO.FileUtils.XmlFileRead<SpecialDays>(insp.Utility.IO.FileUtils.GetDirectory() + "\\specialdays.xml",Encoding.UTF8);
            if(specialDays == null)
                specialDays = new SpecialDays();
        }
        /// <summary>
        /// 是否是工作日
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static bool IsWorkDay(DateTime d)
        {
            SpecialDay sd = specialDays.Items.FirstOrDefault<SpecialDay>(x => x.DateValue == d);
            if(sd != null)
            {
                return sd.DayType == WorkDayType.Work;
            }

            return (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday);

        }
        /// <summary>
        /// 日期内的工作日数
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static int WorkDayCount(DateTime begin,DateTime end)
        {
            int count = 0;
            if(begin > end)
            {
                DateTime t = begin;
                begin = end;
                end = t;
            }
            for(DateTime t = begin;t<=end;t=t.AddDays(1))
            {
                SpecialDay sd = specialDays.Items.FirstOrDefault<SpecialDay>(x => x.DateValue == t);
                if (sd == null && (t.DayOfWeek == DayOfWeek.Saturday || t.DayOfWeek == DayOfWeek.Sunday))
                    continue;
                if (sd != null && sd.DayType != WorkDayType.Work)
                    continue;
                count += 1;
            }
            return count;
        }
        /// <summary>
        /// 取得所有工作日
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static List<DateTime> WorkDayList(DateTime begin, DateTime end)
        {
            List<DateTime> list = new List<DateTime>();
            for (DateTime t = begin; t <= end; t = t.AddDays(1))
            {
                SpecialDay sd = specialDays.Items.FirstOrDefault<SpecialDay>(x => x.DateValue == t);
                if (sd == null && (t.DayOfWeek == DayOfWeek.Saturday || t.DayOfWeek == DayOfWeek.Sunday))
                    continue;
                if (sd != null && sd.DayType != WorkDayType.Work)
                    continue;
                list.Add(t);
            }
            return list;
        }

        public static List<DateTime> WorkWeekList(DateTime begin, DateTime end, params DayOfWeek[] dayOfWeeks)
        {
            List<DateTime> list = new List<DateTime>();
            for (DateTime t = begin; t <= end; t = t.AddDays(1))
            {
                SpecialDay sd = specialDays.Items.FirstOrDefault<SpecialDay>(x => x.DateValue == t);
                if (sd == null && (t.DayOfWeek == DayOfWeek.Saturday || t.DayOfWeek == DayOfWeek.Sunday))
                    continue;
                if (sd != null && sd.DayType != WorkDayType.Work)
                    continue;
                if(dayOfWeeks != null && dayOfWeeks.Contains(t.DayOfWeek))
                    list.Add(t);
            }
            return list;
        }
        #endregion

        #region 时间周期管理
        public static DateTime TimeUnitNextDate(DateTime d, TimeUnit tu)
        {
            if (tu <= TimeUnit.day) return d;
            if (tu == TimeUnit.week)
                return d.AddDays(7);
            throw new NotImplementedException();
        }
        
        public static bool TimeUnitFirst(DateTime d,TimeUnit tu)
        {
            return TimeUnitGetSerialNo(d, tu) == 1;
        }
        
        /// <summary>
        /// 判断d是时间周期tu的序号
        /// </summary>
        /// <param name="d"></param>
        /// <param name="tu"></param>
        /// <returns></returns>
        public static int TimeUnitGetSerialNo(DateTime d, TimeUnit tu)
        {
            if (tu == TimeUnit.day)
                return 1;
            if (tu == TimeUnit.week)
                return d.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)d.DayOfWeek;
            if (tu == TimeUnit.month)
                return d.Day;
            throw new NotImplementedException();

        }
        /// <summary>
        /// 找到d所在的时间周期的时间范围
        /// </summary>
        /// <param name="d"></param>
        /// <param name="tu"></param>
        /// <returns></returns>
        public static DateTime[] TimeUnitRange(DateTime d, TimeUnit tu)
        {
            if (tu <= TimeUnit.day) return new DateTime[] { d, d };
            if (tu == TimeUnit.week)
                return new DateTime[] { GetWeek(d, DayOfWeek.Monday), GetWeek(d, DayOfWeek.Sunday) };
            throw new NotImplementedException();
        }
        /// <summary>
        /// 找到d所在的时间周期的第sn个
        /// </summary>
        /// <param name="d"></param>
        /// <param name="tu"></param>
        /// <param name="sn">从1开始的序号</param>
        /// <returns></returns>
        public static DateTime TimeUnitDate(DateTime d,TimeUnit tu,int sn)
        {
            if (tu <= TimeUnit.day) return d;
            if (tu == TimeUnit.week)
                return GetWeek(d,(sn==7? DayOfWeek.Sunday:(DayOfWeek)sn));
            throw new NotImplementedException();
        }
        /// <summary>
        /// 得到d日的同星期的星期week的日期
        /// </summary>
        /// <param name="d"></param>
        /// <param name="week"></param>
        /// <returns></returns>
        public static DateTime GetWeek(DateTime d, DayOfWeek week)
        {            
            if (d.DayOfWeek == week) return d;
            int diff = (week == DayOfWeek.Sunday?7:(int)week) - (d.DayOfWeek== DayOfWeek.Sunday?7:(int)d.DayOfWeek);            
            return d.AddDays(diff);
        }

        #endregion
    }
    
}
