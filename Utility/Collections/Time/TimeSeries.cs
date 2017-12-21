using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using insp.Utility.Reflection;
using insp.Utility.Bean;
using insp.Utility.Date;
using insp.Utility.Text;

namespace insp.Utility.Collections.Time
{

    /// <summary>
    /// 时间单位
    /// </summary>
    public enum TimeUnit
    {        
        day = 1,
        week = 2,
        month = 3,
        quarter = 4,
        year = 5,

        hour = 6,
        minute = 7,
        second = 8,
        millsecond = 9
    }
    /// <summary>
    /// 时间单位工具
    /// </summary>
    public static class TimeUnitUtils
    {
        /// <summary>
        /// 每种TimeUnit相对于millsecond的倍数
        /// </summary>
        private static Dictionary<TimeUnit, long> scales;

        /// <summary>
        /// 字符串与TimeUnit对应字典
        /// </summary>
        private static Dictionary<TimeUnit, String[]> units;

        /// <summary>
        /// 静态初始化
        /// </summary>
        static TimeUnitUtils()
        {
            scales = CollectionUtils.AsDictionary<TimeUnit, long>(
                TimeUnit.year, (long)365 * 24 * 60 * 60 * 1000,
                TimeUnit.quarter, (long)3 * 30 * 24 * 60 * 60 * 1000,
                TimeUnit.month, (long)30 * 24 * 60 * 60 * 1000,
                TimeUnit.week, (long)7 * 24 * 60 * 60 * 1000,
                TimeUnit.day, (long)24 * 60 * 60 * 1000,
                TimeUnit.hour, (long)60 * 60 * 1000,
                TimeUnit.minute, (long)60 * 1000,
                TimeUnit.second, (long)1000,
                TimeUnit.millsecond, (long)1
            );

            units = CollectionUtils.AsDictionary<TimeUnit, String[]>(
                TimeUnit.year, new String[] { "年", "y", "year", "years" },
                TimeUnit.quarter, new String[] { "季度", "q", "quarters", "quarter" },
                TimeUnit.month, new String[] { "月", "m", "months", "month" },
                TimeUnit.week, new String[] { "周", "w", "weeks", "week" },
                TimeUnit.day, new String[] { "日", "d", "days", "day" },
                TimeUnit.hour, new String[] { "时", "小时", "h", "hours", "hour" },
                TimeUnit.minute, new String[] { "分", "分钟", "m", "minutes", "minute" },
                TimeUnit.second, new String[] { "秒", "秒钟", "s", "seconds", "second" },
                TimeUnit.millsecond, new String[] { "毫秒", "ms", "millsecond", "millseconds" }
            );

            ConvertUtils.RegisteConvertor<String, TimeUnit>(Parse);
        }
        
        /// <summary>
        /// 解析方法 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="Format"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static TimeUnit Parse(String str,String Format="",Properties context=null)
        {
            if (str == null || str == "") throw new Exception("解析TimeUnit失败:空字符串");
            foreach(KeyValuePair<TimeUnit,String[]> pair in units)
            {
                foreach(String s in pair.Value)
                {
                    if(str.ToLower() == s.ToLower())
                    {
                        return pair.Key;
                    }
                }
            }
            throw new Exception("解析TimeUnit失败，无法识别"+str);
        }

        public static int Transfer(int value,TimeUnit from,TimeUnit to)
        {
            if (from == to)
                return value;
            return (int)((value * scales[from]) / scales[to]);
        }
    }
    
    
    public interface ITimeSeries
    {
        String Code { get; }
        TimeUnit TimeUnit { get; }

        /// <summary>
        /// 取得最近时间的数据（d之前）
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        KeyValuePair<int, ITimeSeriesItem> GetNearest(DateTime d, bool forward = true);

        /// 投影
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="propName"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        TimeSeries<ITimeSeriesItem<K>> Select<K>(string propName, int begin, int end);

        ITimeSeries Zoomout(TimeUnit tu);
        ITimeSeries Zoomout(TimeUnit tu, DateTime begin, DateTime end);
    

        /// <summary>
        /// 取得缺省文件名
        /// </summary>
        /// <returns></returns>
        String getDefaultFileName();
        void Save(String fullfilename);
        void Save(String fullfilename, Encoding encode);
        void Save(String fullfilename, Encoding encode, String valueFormat);
        void Save(String fullfilename, Encoding encode, String[] propertyNames, bool columnHeader = false);
        void Save(String fullfilename, Encoding encode, String valueFormat, String[] propertyNames, bool columnHeader = false);
        void Save(String fullfilename, Encoding encode, String valueFormat, String fileFormat, String[] columns, bool columnHeader = false);
        void Load(String fullfilename, bool hasColumnHeader = false, String sep = ",", String[] columns = null, int startRow = 0);
        void Load(String[] strs, String[] columns = null, int startRow = 0, String sep = ",");
    }
    public interface ITimeSeries<T> : ITimeSeries,IList<T>,ICollection<T> where T : ITimeSeriesItem
    {
        /// <summary>
        /// 日期索引器
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        T this[DateTime d] { get;set; }


        /// <summary>
        /// 取时间范围
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        TimeSeries<T> this[DateTime begin, DateTime end] { get;}

        /// <summary>
        /// 取序号范围
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        TimeSeries<T> this[int begin, int end] { get; }

        /// <summary>
        /// 最后一条数据
        /// </summary>
        /// <returns></returns>
        T max();

        /// <summary>
        /// 第一条数据
        /// </summary>
        /// <returns></returns>
        T min();

        T Prev(T cur, int num = 1);
        T Next(T cur, int num = 1);

        Func<List<T>, T> getElementMerger();
        
    }
        
    

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TimeSeries<T> : List<T>, ITimeSeries<T> where T : ITimeSeriesItem
    {
        #region 属性
        /// <summary>
        /// 编码
        /// </summary>
        internal String code;
        /// <summary>
        /// 编码
        /// </summary>
        public String Code { get { return code; } set { code = value; } }

        /// <summary>
        /// 时间单位
        /// </summary>
        internal TimeUnit timeUnit = TimeUnit.day;
        /// <summary>
        /// 时间单位
        /// </summary>
        public TimeUnit TimeUnit { get { return timeUnit; } }
        
        #endregion

        #region 初始化
        /// <summary>
        /// 构造方法
        /// </summary>
        public TimeSeries() { }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="code"></param>
        /// <param name="peroid"></param>
        public TimeSeries(String code,TimeUnit peroid) { this.code = code; this.timeUnit = peroid; }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="code"></param>
        /// <param name="peroid"></param>
        /// <param name="items"></param>
        public TimeSeries(String code, TimeUnit peroid, List<T> items) {
            this.code = code;
            this.timeUnit = peroid;
            this.AddRange(items); }

        #endregion


        #region 数据选择
        /// <summary>
        /// 日期索引器
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public T this[DateTime d]
        {
            get { return this.FirstOrDefault<T>(x => x.Date == d); }
            set { T t = this[d];if (t == null) return;int index = this.IndexOf(t);this[index] = value;}
        }

        
        /// <summary>
        /// 取时间范围
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public TimeSeries<T> this[DateTime begin, DateTime end]
        {
            get
            {
                return new TimeSeries<T>(this.Code,this.TimeUnit,this.Where<T>(x => x.Date >= begin && x.Date <= end).ToList());
            }
        }
        /// <summary>
        /// 取序号范围
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public TimeSeries<T> this[int begin, int end]
        {
            get
            {
                if (begin < 0) begin = 0;
                if (end < 0 || end >= this.Count) end = this.Count - 1;
                return new TimeSeries<T>(this.Code, this.TimeUnit, this.Where<T>((x,index) => index >= begin && index <= end).ToList());
            }
        }

        #endregion

        #region 数据查找
        /// <summary>
        /// 查找突破边界的位置
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="beginDate">开始日期，在开始日期的值一定是低于突破阈值的</param>
        /// <param name="boundylineValue">突破阈值</param>
        /// <param name="rough">突破阈值天数，0表示需突破一天，1</param>
        /// <returns></returns>
        public T FindBoundayLineValue(DateTime beginDate,double boundylineValue,int rough,Func<T,double,double> comparasion, out DateTime endDate,int maxdays=0)
        {
            int sIndex = IndexOf(beginDate);
            if (sIndex < 0)
                sIndex = 0;
            int prevIndex = sIndex;
            int breakthroughDays = 0;
            int endIndex = maxdays == 0 ? this.Count - 1 : sIndex + maxdays;
            for (int i=sIndex;i<= endIndex; i++)
            {
                if (i >= this.Count) break;
                double c = comparasion(this[i], boundylineValue);
                if (c >= 0)//出现突破
                {
                    if (rough <= 0) //一旦出现突破，返回数据
                    {
                        endDate = this[i].Date;
                        return this[i];
                    }
                        
                    if (breakthroughDays >= rough)
                    {
                        endDate = this[i].Date;
                        return this[i];
                    }
                        
                    breakthroughDays += 1;
                }
                else
                {
                    breakthroughDays = 0;
                }
                prevIndex = i;
            }
            if(endIndex >= this.Count)
                endDate = this[this.Count-1].Date;
            else
                endDate = this[endIndex].Date;
            return default(T);
        }

        /// <summary>
        /// 查找突破边界的位置
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="beginDate">开始日期，在开始日期的值一定是低于突破阈值的</param>
        /// <param name="boundylineValue">突破阈值</param>
        /// <param name="rough">突破阈值天数，0表示需突破一天，1</param>
        /// <returns></returns>
        public T FindBoundayLineValue2(DateTime beginDate, double[] boundylineValue, int rough, Func<T, double, double> comparasion, out DateTime endDate, int maxdays = 0)
        {
            int sIndex = IndexOf(beginDate);
            if (sIndex < 0)
                sIndex = 0;
            int prevIndex = sIndex;
            int breakthroughDays = 0;
            int endIndex = maxdays == 0 ? this.Count - 1 : sIndex + maxdays;
            for (int i = sIndex; i <= endIndex; i++)
            {
                if (i >= this.Count) break;
                for (int j = 0; j < boundylineValue.Length; j++)
                {
                    double c = comparasion(this[i], boundylineValue[j]);
                    if (c >= 0)//出现突破
                    {
                        if (rough <= 0) //一旦出现突破，返回数据
                        {
                            endDate = this[i].Date;
                            return this[i];
                        }

                        if (breakthroughDays >= rough)
                        {
                            endDate = this[i].Date;
                            return this[i];
                        }

                        breakthroughDays += 1;
                    }
                    
                }
                breakthroughDays = 0;
                prevIndex = i;
            }
            if (endIndex >= this.Count)
                endDate = this[this.Count - 1].Date;
            else
                endDate = this[endIndex].Date;
            return default(T);
        }

        /// <summary>
        /// 查找突破边界并维持突破状态的位置
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="beginDate"></param>
        /// <param name="boundylineValue"></param>
        /// <returns></returns>
        public T FindBoundayLineAndMaintainValue(DateTime beginDate, double boundylineValue, Func<T, double, int> comparasion,int maintainDays=1)
        {
            int sIndex = IndexOf(beginDate);
            if (sIndex < 0)
                sIndex = 0;
            int prevIndex = sIndex;
            for (int i = sIndex; i < this.Count; i++)
            {
                double c = comparasion(this[i], boundylineValue);
                if (c > 0)
                    return this[i];
                prevIndex = i;
            }
            return default(T);
        }
        public int IndexOf(DateTime time)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Date == time)
                    return i;
            }
            return -1;
        }
        
        /// <summary>
        /// 取得最近时间的数据（d之前）
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public KeyValuePair<int,ITimeSeriesItem> GetNearest(DateTime d,bool forward=true)
        {
            for(int i=0;i<this.Count-1;i++)
            {
                if (d == this[i].Date)
                    return new KeyValuePair<int, ITimeSeriesItem>(i, this[i]);
                if(d == this[i+1].Date)
                    return new KeyValuePair<int, ITimeSeriesItem>(i+1, this[i+1]);
                if (d > this[i].Date && d < this[i + 1].Date)
                    return forward?new KeyValuePair<int, ITimeSeriesItem>(i, this[i]): new KeyValuePair<int, ITimeSeriesItem>(i + 1, this[i + 1]);
            }
            if (forward)
                return new KeyValuePair<int, ITimeSeriesItem>(this.Count - 1, this[this.Count - 1]);
            return new KeyValuePair<int, ITimeSeriesItem>(-1,null);
        }

        
        #endregion

        #region 投影
        /// <summary>
        /// 投影
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="propName"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public TimeSeries<ITimeSeriesItem<K>> Select<K>(string propName, int begin, int end) 
        {
            if (begin < 0) begin = 0;
            if (end <= 0 || end >= this.Count) end = this.Count - 1;

            
            return new TimeSeries<ITimeSeriesItem<K>>(Code, TimeUnit,
                GetRange(begin, end - begin + 1).Select(x => x.Select<K>(propName)).ToList());
        }

        /// <summary>
        /// 最后一条数据
        /// </summary>
        /// <returns></returns>
        public T max()
        {
            //this.Sort();            
            return this[Count-1];
        }
        /// <summary>
        /// 第一条数据
        /// </summary>
        /// <returns></returns>
        public T min()
        {
            //this.Sort();
            return this[0];
        }

        public T Prev(T cur,int num=1)
        {
            for(int i=0;i<this.Count;i++)
            {
                if (this[i].Equals(cur))
                    return i-num < 0 ? default(T) : this[i - num];
            }
            return default(T);
        }
        public T Next(T cur,int num =1)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Equals(cur))
                    return i+num >= this.Count-1 ? default(T) : this[i + num];
            }
            return default(T);
        }

        public virtual Func<List<T>, T> getElementMerger()
        {
            return null;
        }
        public virtual ITimeSeries Zoomout(TimeUnit tu)
        {
            if (this.Count <= 0) return null;
            return Zoomout(tu, this[0].Date, this[this.Count - 1].Date);
        }
        public virtual ITimeSeries Zoomout(TimeUnit tu,DateTime begin,DateTime end)
        {
            Func<List<T>, T> merger = this.getElementMerger();
            if (merger == null) return null;
            if (this.Count <= 0) return null;
            if (begin < this[0].Date) begin = this[0].Date;
            if (end > this[this.Count - 1].Date) end = this[this.Count - 1].Date;

            List<T> items = new List<T>();
            TimeSeries<T> result = (TimeSeries<T>)this.GetType().GetConstructor(new Type[] { typeof(String), typeof(TimeUnit) }).Invoke(new Object[] { code, tu });
            
            /*
            DateTime d = begin;
            KeyValuePair<int, ITimeSeriesItem> kv = this.GetNearest(d);
            if (kv.Value == null)
                return null;
            int index = kv.Key;
            while(index < this.Count)
            {
                int n = CalendarUtils.TimeUnitGetSerialNo(this[index].Date,tu);
                if (n == 1)
                    break;
                index += 1;
            }

            DateTime[] dayRange = null;
            for (int i=index;i<this.Count;i++)
            {
                d = this[i].Date;
                if(dayRange == null)
                    dayRange = CalendarUtils.TimeUnitRange(d, tu);
                if(d<=dayRange[1])
                {
                    items.Add(this[i]);
                    continue;
                }
                T newItem = merger(items);
                if (newItem != null)
                    result.Add(newItem);
                dayRange = null;

            }
            */

            DateTime d = begin;
            while(d <= end)
            {
                DateTime[] dayRange = CalendarUtils.TimeUnitRange(d,tu); //找到d所在的周期的第一天
                KeyValuePair<int, ITimeSeriesItem> kvBegin = this.GetNearest(dayRange[0]);
                KeyValuePair<int, ITimeSeriesItem> kvEnd = this.GetNearest(dayRange[1]);
                items.Clear();
                for (int i = kvBegin.Key; i <= kvEnd.Key; i++)
                    items.Add(this[i]);
                T newItem = merger(items);
                if (newItem != null)
                    result.Add(newItem);
                d = CalendarUtils.TimeUnitNextDate(d, tu);
            }
            
            return result;
        }
        #endregion

        #region 运算
        /// <summary>
        /// 加法
        /// </summary>
        /// <param name="ts1"></param>
        /// <param name="ts2"></param>
        /// <returns></returns>
        public static TimeSeries<ITimeSeriesItem<double>> operator+(TimeSeries<T> ts1, TimeSeries<T> ts2)
        {            
            TimeSeries<ITimeSeriesItem<double>> results = new TimeSeries<ITimeSeriesItem<double>>();
            ts1.ForEach(t1 => results.Add(t1 as TimeSeriesItem + (ts2[t1.Date] as TimeSeriesItem)));
            return results;
        }
        /// <summary>
        /// 加法
        /// </summary>
        /// <param name="ts1"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static TimeSeries<ITimeSeriesItem<double>> operator +(TimeSeries<T> ts1, double v)
        {
            TimeSeries<ITimeSeriesItem<double>> results = new TimeSeries<ITimeSeriesItem<double>>();
            ts1.ForEach(t1 => results.Add(t1 as TimeSeriesItem + v));
            return results;
        }
        /// <summary>
        /// 减法
        /// </summary>
        /// <param name="ts1"></param>
        /// <param name="ts2"></param>
        /// <returns></returns>
        public static TimeSeries<ITimeSeriesItem<double>> operator -(TimeSeries<T> ts1, TimeSeries<T> ts2)
        {
            TimeSeries<ITimeSeriesItem<double>> results = new TimeSeries<ITimeSeriesItem<double>>();
            ts1.ForEach(t1 => results.Add(t1 as TimeSeriesItem - (ts2[t1.Date] as TimeSeriesItem)));
            return results;
        }

        /// <summary>
        /// 减法
        /// </summary>
        /// <param name="ts1"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static TimeSeries<ITimeSeriesItem<double>> operator -(TimeSeries<T> ts1, double v)
        {
            TimeSeries<ITimeSeriesItem<double>> results = new TimeSeries<ITimeSeriesItem<double>>();
            ts1.ForEach(t1 => results.Add(t1 as TimeSeriesItem - v));
            return results;
        }
        /// <summary>
        /// 乘法
        /// </summary>
        /// <param name="ts1"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static TimeSeries<ITimeSeriesItem<double>> operator *(TimeSeries<T> ts1, double v)
        {
            TimeSeries<ITimeSeriesItem<double>> results = new TimeSeries<ITimeSeriesItem<double>>();
            ts1.ForEach(t1 => results.Add((t1 as TimeSeriesItem) * v));
            return results;
        }
        /// <summary>
        /// 除法
        /// </summary>
        /// <param name="ts1"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static TimeSeries<ITimeSeriesItem<double>> operator /(TimeSeries<T> ts1, double v)
        {
            TimeSeries<ITimeSeriesItem<double>> results = new TimeSeries<ITimeSeriesItem<double>>();
            ts1.ForEach(t1 => results.Add(t1 as TimeSeriesItem / v));
            return results;
        }
        /// <summary>
        /// 除法
        /// </summary>
        /// <param name="ts1"></param>
        /// <param name="ts2"></param>
        /// <returns></returns>
        public static TimeSeries<ITimeSeriesItem<double>> operator /(TimeSeries<T> ts1, TimeSeries<T> ts2)
        {
            TimeSeries<ITimeSeriesItem<double>> results = new TimeSeries<ITimeSeriesItem<double>>();
            ts1.ForEach(t1 => results.Add(t1 as TimeSeriesItem  / (ts2[t1.Date] as TimeSeriesItem)));
            return results;
        }
        #endregion

        #region 文件读写
        
        /// <summary>
        /// 取得缺省文件名
        /// </summary>
        /// <returns></returns>
        public String getDefaultFileName()
        {
            return this.code + "." + this.TimeUnit.ToString();
        }
        public void Save(String fullfilename)
        {
            Save(fullfilename, Encoding.UTF8, "", ConvertUtils.TEXT_FMT_CSV, null, false);
        }
        public void Save(String fullfilename, Encoding encode)
        {
            Save(fullfilename, encode, "", ConvertUtils.TEXT_FMT_CSV, null, false);
        }
        public void Save(String fullfilename, Encoding encode, String valueFormat)
        {
            Save(fullfilename, encode, valueFormat, ConvertUtils.TEXT_FMT_CSV, null, false);
        }
        public void Save(String fullfilename, Encoding encode, String[] propertyNames, bool columnHeader = false)
        {
            Save(fullfilename, encode, "", ConvertUtils.TEXT_FMT_CSV, propertyNames, columnHeader);
        }
        public void Save(String fullfilename, Encoding encode, String valueFormat, String[] propertyNames, bool columnHeader = false)
        {
            Save(fullfilename, encode, valueFormat, ConvertUtils.TEXT_FMT_CSV, propertyNames, columnHeader);
        }

        public void Save(String fullfilename, Encoding encode, String valueFormat, String fileFormat, String[] columns,bool columnHeader=false)
        {
            
            String[] strs = ConvertAll<String>(item => ((TimeSeriesItem)(Object)item).ToText(valueFormat, fileFormat, columns)).ToArray<String>();
            if (strs == null || strs.Length <= 0)
                return;
            if (columnHeader)
                System.IO.File.WriteAllText(fullfilename, columns.Merge(), encode);
            System.IO.File.AppendAllLines(fullfilename, strs, encode);

        }


        public void Load(String fullfilename, bool hasColumnHeader=false,String sep=",",String[] columns = null,int startRow=0)
        {
            if (!System.IO.File.Exists(fullfilename))
                return;
            String[] strs = System.IO.File.ReadAllLines(fullfilename,Encoding.UTF8);
            if (strs == null || strs.Length <= 0 || strs[0] == null || strs[0].Trim() == "")
                return;

            int start = 0;
            
            if (hasColumnHeader && columns == null)
            {
                columns = strs[0].Split(',');
                start = 1;
            }
            if(strs[0].Contains("时间") || strs[0].Contains("日期"))
            {
                columns = strs[0].Split(',');
                start = 1;
            }
            if (startRow <= 0)
                startRow = start;

            Load(strs, columns, startRow,sep);

            
        }

        public void Load(String[] strs, String[] columns = null, int startRow = 0,String sep = ",")
        {
            if (strs == null || strs.Length <= 0 || strs[0] == null || strs[0].Trim() == "")
                return;            
            this.Clear();            
            PropertyDescriptorCollection pdc = PropertyObjectUtils.GetPropertyDescriptorCollection(typeof(T));


            for (int i = startRow; i < strs.Length; i++)
            {
                if (strs[i] == null || strs[i] == "")
                    continue;
                String s = strs[i].Trim();
                if (s == "")
                    continue;
                T t = TimeSeriesItemUtils.Parse<T>(strs[i], columns);
                if (t != null)
                    this.Add(t);
            }
           
        }

        
        #endregion


    }
    
}
