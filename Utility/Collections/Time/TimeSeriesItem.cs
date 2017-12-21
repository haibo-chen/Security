using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Bean;


namespace insp.Utility.Collections.Time
{
    /// <summary>
    /// 时序数据项
    /// </summary>
    public abstract class TimeSeriesItem : ITimeSeriesItem
    {
        #region 时间
        
        /// <summary>
        /// 日期
        /// </summary>
        public virtual DateTime Date { get { return this.GetValue<DateTime>("time"); } set { this.SetValue<DateTime>("time", value); } }

        /// <summary>
        /// 值数组
        /// </summary>
        protected Object[] values;
        /// <summary>
        /// 初始化值数组
        /// </summary>
        protected void init()
        {
            PropertyDescriptorCollection pds = this.GetPropertyDescriptorCollection();
            if (pds == null)
                pds = createDefaultPropertyDescriptor();
            
            values = new Object[pds.Count];
        }

        public TimeSeriesItem()
        {
            init();
        }

        protected PropertyDescriptorCollection createDefaultPropertyDescriptor()
        {
            PropertyDescriptorCollection pdc = new PropertyDescriptorCollection();
            pdc.Add(new PropertyDescriptor() { Name = "time", Caption = "时间", TypeName = "DateTime" });
            pdc.Add(new PropertyDescriptor() { Name = "value", Caption = "值", TypeName = "Object" });
            return pdc;
        }
        #endregion

        #region 方法重写
        /// <summary>
        /// 取得属性描述符
        /// </summary>
        /// <returns></returns>
        public abstract PropertyDescriptorCollection GetPropertyDescriptorCollection();
        
        /// <summary>
        /// 取得缺省值
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <returns></returns>
        public abstract K GetDefaultValue<K>();

        /// <summary>
        /// 设置缺省值
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <returns></returns>
        public abstract void SetDefaultValue(Object value);
        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        public Object this[String propName]
        {
            get { return this.GetValue<Object>(propName); }
            set { this.SetValue<Object>(propName,value); }
        }

        /// <summary>
        /// 取特定属性值
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="propName"></param>
        /// <returns></returns>
        public virtual K GetValue<K>(String propName)
        {
            int index = IndexOf(propName);
            if (index < 0) return default(K);
            if(values[index] == null) return default(K);
            if (typeof(K) == values[index].GetType() || typeof(K).IsAssignableFrom(values[index].GetType()))
                return (K)values[index];
            return ConvertUtils.ConvertTo<K>(values[index]);
        }
        /// <summary>
        /// 查找属性位置
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        protected int IndexOf(String propName)
        {
            if (values == null) return -1;
            if (values.Length == 1) return 0;
            PropertyDescriptorCollection pds = this.GetPropertyDescriptorCollection();
            return pds.IndexOf(propName);

        }
        /// <summary>
        /// 设置特定值
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="propname"></param>
        /// <param name="value"></param>
        public virtual void SetValue<K>(String propname, K value)
        {
            int index = IndexOf(propname);
            if (index < 0) values[0] = value;
            values[index] = value;
        }

        public int CompareTo(ITimeSeriesItem other)
        {
            return (int)(this.Date.Ticks - other.Date.Ticks);
        }

        public ITimeSeriesItem<K> Select<K>(String propName)
        {
            return new TimeSeriesItem<K>(this.Date, this.GetValue<K>(propName));
        }

        #endregion

        #region 算术运算
        public static TimeSeriesItem<double> operator+(TimeSeriesItem a, TimeSeriesItem b)
        {
            if (a == null && b == null) return new TimeSeriesItem<double>();
            else if (a == null) return new TimeSeriesItem<double>() { Date = b.Date, Value = b.GetDefaultValue<double>() };
            else if (b == null) return new TimeSeriesItem<double>() { Date = a.Date, Value = a.GetDefaultValue<double>() };
            return new TimeSeriesItem<double>()
            {
                Date = a.Date,
                Value = a.GetDefaultValue<double>() + b.GetDefaultValue<double>()
            };            
        }

        

        public static TimeSeriesItem<double> operator -(TimeSeriesItem a, TimeSeriesItem b)
        {
            if (a == null && b == null) return new TimeSeriesItem<double>();
            else if (a == null) return new TimeSeriesItem<double>() { Date = b.Date, Value = b.GetDefaultValue<double>() };
            else if (b == null) return new TimeSeriesItem<double>() { Date = a.Date, Value = a.GetDefaultValue<double>() };
            return new TimeSeriesItem<double>()
            {
                Date = a.Date,
                Value = a.GetDefaultValue<double>() - b.GetDefaultValue<double>()
            };
        }

        public static TimeSeriesItem<double> operator *(TimeSeriesItem a, TimeSeriesItem b)
        {
            if (a == null && b == null) return new TimeSeriesItem<double>();
            else if (a == null) return new TimeSeriesItem<double>() { Date = b.Date, Value = b.GetDefaultValue<double>() };
            else if (b == null) return new TimeSeriesItem<double>() { Date = a.Date, Value = a.GetDefaultValue<double>() };
            return new TimeSeriesItem<double>()
            {
                Date = a.Date,
                Value = a.GetDefaultValue<double>() * b.GetDefaultValue<double>()
            };
        }

        public static TimeSeriesItem<double> operator /(TimeSeriesItem a, TimeSeriesItem b)
        {
            if (a == null && b == null) return new TimeSeriesItem<double>();
            else if (a == null) return new TimeSeriesItem<double>() { Date = b.Date, Value = b.GetDefaultValue<double>() };
            else if (b == null) return new TimeSeriesItem<double>() { Date = a.Date, Value = a.GetDefaultValue<double>() };
            return new TimeSeriesItem<double>()
            {
                Date = a.Date,
                Value = a.GetDefaultValue<double>() / b.GetDefaultValue<double>()
            };
        }


        public static TimeSeriesItem<double> operator +(TimeSeriesItem a, double b)
        {
            return new TimeSeriesItem<double>()
            {
                Date = a.Date,
                Value = a.GetDefaultValue<double>() + b
            };
        }

        public static TimeSeriesItem<double> operator -(TimeSeriesItem a, double b)
        {
            return new TimeSeriesItem<double>()
            {
                Date = a.Date,
                Value = a.GetDefaultValue<double>() - b
            };
        }

        public static TimeSeriesItem<double> operator *(TimeSeriesItem a, double b)
        {
            return new TimeSeriesItem<double>()
            {
                Date = a.Date,
                Value = a.GetDefaultValue<double>() * b
            };
        }

        public static TimeSeriesItem<double> operator /(TimeSeriesItem a, double b)
        {
            return new TimeSeriesItem<double>()
            {
                Date = a.Date,
                Value = a.GetDefaultValue<double>() / b
            };
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //this.DoDispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~TimeSeriesItem() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }


        #endregion


        #region 字符串处理

        

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public void Parse(String s)
        {
            if (s == null || s == "")
                return;            
            String[] ss = s.Split(',');
            if (ss == null || ss.Length <= 0)
                return;

            PropertyDescriptorCollection pds = this.GetPropertyDescriptorCollection();
            for (int i=0;i<ss.Length;i++)
            {
                if (ss[i] == null || ss[i].Trim() == "")
                    continue;
                String[] itemstr = ss[i].Split('=');
                if (itemstr == null || itemstr.Length <= 0)
                    continue;
                if (itemstr[0] == null || itemstr[0].Trim() == "")
                    continue;
                if (itemstr.Length<1 || itemstr[1] == null || itemstr[1].Trim() == "")
                    continue;
                int index = IndexOf(itemstr[0].Trim());
                if (index < 0) continue;
                PropertyDescriptor pd = pds[index];
                values[index] = ConvertUtils.ConvertTo(itemstr[1].Trim(), pd.Type, pd.Format);
            }            
        }

        /// <summary>
        /// 解析字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T Parse<T>(String str, String sep = ",", String[] columns = null) where T : ITimeSeriesItem
        {
            if (str == null || str == "") return default(T);
            Comparison<PropertyDescriptor> comparison = (x, y) => x.xh - y.xh;
            PropertyDescriptorCollection pdc = PropertyObjectUtils.GetPropertyDescriptorCollection(typeof(T));
            pdc.Sort(comparison);
            List<PropertyDescriptor> pdList = new List<PropertyDescriptor>();
            if (columns != null && columns.Length > 0)
            {
                for (int i = 0; i < columns.Length; i++)
                {
                    PropertyDescriptor pd = pdc.Get(columns[i]);
                    if (pd != null) pdList.Add(pd);
                }
            }
            else
            {
                pdList.AddRange(pdc);
            }

            String[] ss = str.Split(sep.ToCharArray());
            if (ss == null || ss.Length <= 0) return default(T);

            T instance = (T)typeof(T).GetConstructor(new Type[0]).Invoke(null);
            for (int i = 0; i < ss.Length; i++)
            {
                Object value = ConvertUtils.ConvertTo(ss[i], pdList[i].Type);
                instance[pdList[i].Name] = value;
            }
            return instance;
        }
        #endregion


    }
    /// <summary>
    /// 时序列项基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TimeSeriesItem<T> : TimeSeriesItem, ITimeSeriesItem<T>
    {

        #region 重写ITimeSeriesItem<T>
        /// <summary>
        /// 日期
        /// </summary>
        public override DateTime Date { get { return (DateTime)this.values[0]; } set { this.values[0] = value ; } }


        /// <summary>
        /// 值
        /// </summary>
        public T Value { get { return (T)values[1]; } set { this.values[1] = (T)value; } }

        #endregion

        #region 初始化
        public TimeSeriesItem() { }
        public TimeSeriesItem(DateTime d,T value)
        {
            this.Date = d;
            this.Value = value;
        }
        #endregion

        #region 重写ITimeSeriesItem
        /// <summary>
        /// 取得缺省值
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <returns></returns>
        public override K GetDefaultValue<K>()
        {
            if (typeof(K) == typeof(T))
                return (K)(Object)Value;
            return ConvertUtils.ConvertTo<T, K>(Value);
        }
        /// <summary>
        /// 设置缺省值
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <returns></returns>
        public override void SetDefaultValue(Object value)
        {
            if (value == null)
            {
                this.values[1] = null;
                return;
            }
                

            if(typeof(T) == value.GetType())
            {
                this.values[1] = (T)value;
                return;
            }

            this.values[1] = ConvertUtils.ConvertTo<T>(value);
        }


        /// <summary>
        /// 取特定属性值
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="propName"></param>
        /// <returns></returns>
        public override K GetValue<K>(String propName)
        {
            if (typeof(K) == typeof(T))
                return (K)(Object)Value;
            return ConvertUtils.ConvertTo<T, K>(Value);
        }
        /// <summary>
        /// 设置特定值
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="propname"></param>
        /// <param name="value"></param>
        public override void SetValue<K>(String propname, K value)
        {
            if (typeof(K) == typeof(T))
                this.values[1] = (T)(Object)value;
            else
                this.values[1]  = ConvertUtils.ConvertTo<K,T>(value); 

        }

        #endregion

        #region 字符串处理

        /// <summary>
        /// 字符串 
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return "date=" + this.Date.ToString("yyyy-MM-dd hh:mm:ss") + ",value=" + ConvertUtils.objectToStr(values[1]);
        }
        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public new static TimeSeriesItem<T> Parse(String s)
        {
            if (s == null || s == "")
                return null;
            TimeSeriesItem<T> item = new TimeSeriesItem<T>();

            String[] ss = s.Split(',');
            if (ss == null || ss.Length <= 0)
                return null;
            String[] sdate = ss[0].Split('=');
            if (sdate == null || sdate.Length < 2)
                return null;
            item.Date = DateTime.ParseExact(sdate[1].Trim(), "yyyy-MM-dd hh:mm:ss",null);

            String[] svalue = ss[1].Split('=');
            item.values[1] = ConvertUtils.strToObject<T>(svalue[1].Trim());

            return item;
        }

        private static PropertyDescriptorCollection pdc;
        public override PropertyDescriptorCollection GetPropertyDescriptorCollection()
        {
            if(pdc == null)
            {
                pdc = new PropertyDescriptorCollection();
                pdc.Add(new PropertyDescriptor() { Name = "time", Caption = "时间", TypeName = "DateTime" });
                pdc.Add(new PropertyDescriptor() { Name = "value", Caption = "值", TypeName = typeof(T).FullName });
                PropertyObjectUtils.RegisteProperties<TimeSeriesItem<T>>(pdc);
            }
            return pdc;
        }


        #endregion
    }


   
}
