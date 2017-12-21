using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using insp.Utility.Bean;
using insp.Utility.Date;
using insp.Utility.Text;

namespace insp.Utility.Collections.Time
{
    
    /// <summary>
    /// 时间序列数据项
    /// </summary>
    public interface ITimeSeriesItem : IComparable<ITimeSeriesItem>
    {
        /// <summary>
        /// 日期
        /// </summary>
        DateTime Date { get; set; }

        /// <summary>
        /// 取得缺省值
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <returns></returns>
        K GetDefaultValue<K>();
        /// <summary>
        /// 设置缺省值
        /// </summary>
        /// <param name="Value"></param>
        void SetDefaultValue(Object Value);
        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        Object this[String propName] { get; set; }

        /// <summary>
        /// 取特定属性值
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="propName"></param>
        /// <returns></returns>
        K GetValue<K>(String propName);
        /// <summary>
        /// 设置特定值
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="propname"></param>
        /// <param name="value"></param>
        void SetValue<K>(String propname, K value);

        /// <summary>
        /// 投影运算
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="propName"></param>
        /// <returns></returns>
        ITimeSeriesItem<K> Select<K>(String propName);

        /// <summary>
        /// 取得属性描述符
        /// </summary>
        /// <returns></returns>
        PropertyDescriptorCollection GetPropertyDescriptorCollection();
    }

    /// <summary>
    /// 时间序列数据项
    /// </summary>
    public interface ITimeSeriesItem<T> : ITimeSeriesItem
    {
        /// <summary>
        /// 值
        /// </summary>
        T Value { get; set; }
    }

    public static class TimeSeriesItemUtils
    {
        /// <summary>
        /// 缺省的日期时间格式
        /// </summary>
        public static readonly String FMT_TIME = "yyyy-MM-dd hh:mm:ss";

        public static String ToText(this ITimeSeriesItem item)
        {
            return ToText(item, "",ConvertUtils.TEXT_FMT_CSV);
        }
        public static String ToText(this ITimeSeriesItem item, String valueFormat)
        {
            return ToText(item, valueFormat, ConvertUtils.TEXT_FMT_CSV);
        }
        public static String ToText(this ITimeSeriesItem item, params String[] propertyNames)
        {
            return ToText(item, "", ConvertUtils.TEXT_FMT_CSV, propertyNames);
        }
        public static String ToText(this ITimeSeriesItem item, String valueFormat, params String[] propertyNames)
        {
            return ToText(item, valueFormat, ConvertUtils.TEXT_FMT_CSV, propertyNames);
        }
        /// <summary>
        /// 转文本
        /// </summary>
        /// <param name="item"></param>
        /// <param name="valueFormat"></param>
        /// <param name="fileFormat">参见ConvertUtils.TEXT_XXX</param>
        /// <param name="propertyNames"></param>
        /// <returns></returns>
        public static String ToText(this ITimeSeriesItem item, String valueFormat,String fileFormat,params String[] propertyNames)
        {
            if (item == null) return "";
            Object value = item.GetDefaultValue<Object>();
            
            if (fileFormat == null || fileFormat == "") fileFormat = ConvertUtils.TEXT_FMT_CSV;
            PropertyDescriptorCollection pds = item.GetPropertyDescriptorCollection();
            if (pds == null)
            {
                String temp = fileFormat.Replace("{$P}", "date").Replace("{$V}", item.Date.ToString(FMT_TIME));
                
                return temp + ","+ fileFormat.Replace("{$P}", "value").Replace("{$V}", ConvertUtils.objectToStr(value, valueFormat));                
            }

            StringBuilder str = new StringBuilder();
            for (int i = 0; i < pds.Count; i++)
            {
                PropertyDescriptor pd = pds[i];
                if (str.ToString() != "")
                    str.Append(",");
                String temp = fileFormat.Replace("{$P}", pd.Name).Replace("{$V}", ConvertUtils.objectToStr(item.GetValue<Object>(pd.name), pd.Format));                
                str.Append(temp);
            }
            return str.ToString();

        }
        /// <summary>
        /// 解析
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T Parse<T>(String s, String[] propertyNames = null) where T : ITimeSeriesItem
        {
            if (s == null || s == "")
                return default(T);
            String[] columns = s.Split(',');
            if (columns == null || columns.Length <= 1 || columns[0].Trim() == "")
                return default(T); ;


            Type elementType = typeof(T);            
            if(elementType.IsInterface)            
                elementType = typeof(TimeSeriesItem<>).MakeGenericType(typeof(T).GenericTypeArguments);            
            T insance =  (T)typeof(T).Assembly.CreateInstance(elementType.FullName);

            PropertyDescriptorCollection pdc = PropertyObjectUtils.GetPropertyDescriptorCollection(typeof(T));
            if (pdc == null)//属性描述符没有的情况
            {
                if (!typeof(T).IsGenericType || typeof(T).GenericTypeArguments == null || typeof(T).GenericTypeArguments.Length <= 0)
                    return default(T);
                if (columns[0] != null && columns[0].Contains("="))
                    columns[0] = columns[0].Split('=')[1].Trim();
                if (columns[1] != null && columns[1].Contains("="))
                    columns[1] = columns[1].Split('=')[1].Trim();
                insance.Date = DateUtils.Parse(columns[0]);
                Object v = ConvertUtils.strToObject(columns.Merge(1, columns.Length - 1), typeof(T).GenericTypeArguments[0]);
                insance.SetDefaultValue(v);
                return insance;
            }

            for (int i = 0; i < columns.Length; i++)
            {
                if (columns[i] == null || columns[i].Trim() == "")
                    continue;

                PropertyDescriptor pd = pdc[i];
                String value = columns[i].Trim();
                if (columns[i].Contains('='))
                {
                    String[] itemstr = columns[i].Split('=');
                    String propName = itemstr[0] == null ? "" : itemstr[0].Trim();
                    pd = pdc.Get(propName);
                    if (pd == null) pd = pdc[i];
                    value = itemstr[1] == null ? "" : itemstr[1].Trim();
                }

                insance.SetValue(pd.Name, ConvertUtils.ConvertTo(value, pd.Type, pd.Format));
            }
            return insance;
        }
    }










}
