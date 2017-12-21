using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Dynamic;
using System.Collections.Concurrent;

using insp.Utility.Reflection;
using insp.Utility.Collections;
using System.Linq.Expressions;

namespace insp.Utility.Bean
{
    /// <summary>
    /// 带有属性声明的对象
    /// </summary>
    public interface IPropertyObject : IDisposable
    {
        
        Object this[String propName] { get;set; }
    }

    public static class PropertyObjectUtils
    {
        public const String DEFAULT_PROP = "Value";

        #region 内部成员
        /// <summary>
        /// 类型的属性记录
        /// </summary>
        //private static readonly ConcurrentDictionary<Type, PropertyDescriptorCollection> typeProperties = new ConcurrentDictionary<Type, PropertyDescriptorCollection>();
        private static readonly Dictionary<Type, PropertyDescriptorCollection> typeProperties = new Dictionary<Type, PropertyDescriptorCollection>();
        /// <summary>
        /// 对象数据值
        /// </summary>
        //private static readonly ConcurrentDictionary<Object, ConcurrentDictionary<PropertyDescriptor,Object>> objectPropertiesValues = new ConcurrentDictionary<object, ConcurrentDictionary<PropertyDescriptor, Object>>(); 
        private static readonly Dictionary<Object, Dictionary<PropertyDescriptor, Object>> objectPropertiesValues = new Dictionary<object, Dictionary<PropertyDescriptor, Object>>();


        #endregion

        #region 属性集管理
        /// <summary>
        /// 获取属性集
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static PropertyDescriptorCollection GetPropertyDescriptorCollection(this IPropertyObject obj)
        {
            if (typeProperties.ContainsKey(obj.GetType()))
                return typeProperties[obj.GetType()];
            return null;
        }
        public static PropertyDescriptorCollection GetPropertyDescriptorCollection(Type type)
        {
            if (typeProperties.ContainsKey(type))
                return typeProperties[type];
            return null;
        }
        #endregion


        #region 取得或者修改属性值
        /// <summary>
        /// 清除对象
        /// </summary>
        /// <param name="obj"></param>
        public static void DoDispose(this IPropertyObject obj)
        {
            if (!objectPropertiesValues.ContainsKey(obj))
                return;
            //ConcurrentDictionary<PropertyDescriptor, Object> value = null;
            //objectPropertiesValues.TryRemove(obj, out value);
            objectPropertiesValues.Remove(obj);

        }
        /// <summary>
        /// 取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static T GetValue<T>(this IPropertyObject obj,String propName="",bool throwException=false)
        {
            if (propName == "" || propName == null) propName = DEFAULT_PROP;
             //如果propName是普通成员，则直接返回成员值
            MemberInfo member = obj.GetType().FindMember(propName);
            if (member != null)
                return member.FindValue<T>(obj);

            //obj的类型的属性是否已经注册
            PropertyDescriptorCollection pdc = obj.GetPropertyDescriptorCollection();
            if(pdc == null)            
                RegisteProperties(obj);
            pdc = obj.GetPropertyDescriptorCollection();
            //没有可以注册的属性
            if (pdc == null)
            {
                if (throwException) throw new Exception("类型"+obj.GetType().Name+"没有可以注册的属性 ");
                else return default(T);
            }
            PropertyDescriptor pd = pdc.Get(propName);
            int index = pdc.IndexOf(propName);
            if (index < 0)//属性名不存在
            {
                if (throwException) throw new Exception("属性名不存在:"+propName);
                else return default(T);
            }
            if (!objectPropertiesValues.ContainsKey(obj))
                //objectPropertiesValues[obj] = new ConcurrentDictionary<PropertyDescriptor, object>();
                objectPropertiesValues[obj] = new Dictionary<PropertyDescriptor, object>();
            Dictionary<PropertyDescriptor, object> values = objectPropertiesValues[obj];
            if (!values.ContainsKey(pd))
                return default(T);
            return (T)values[pd];            
        }
        /// <summary>
        /// 取缺省值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T GetDefaultValue<T>(this IPropertyObject obj,bool throwException = false)
        {
            PropertyDescriptorCollection pdc = obj.GetPropertyDescriptorCollection();
            if (pdc == null)
                RegisteProperties(obj);
            pdc = obj.GetPropertyDescriptorCollection();
            //没有可以注册的属性
            if (pdc == null)
            {
                if (throwException) throw new Exception("类型" + obj.GetType().Name + "没有可以注册的属性 ");
                else return default(T);
            }            
            PropertyDescriptor pd = pdc.GetDefault();
            if (pd == null)
            {
                if (throwException) throw new Exception("类型" + obj.GetType().Name + "没有缺省属性");
                else return default(T);
            }
            int index = pdc.IndexOf(pd);
            if (index < 0) return default(T);

            if (!objectPropertiesValues.ContainsKey(obj))
                objectPropertiesValues[obj] = new Dictionary<PropertyDescriptor, object>();
            Dictionary<PropertyDescriptor, object> values = objectPropertiesValues[obj];
            return (T)values.GetValue(pd, default(T));            

        }
        /// <summary>
        /// 修改值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetValue(this IPropertyObject obj, String propName, Object value,bool throwException = false)
        {
            if (obj == null)
                return;
            //propName是否是普通成员
            MemberInfo member = obj.GetType().FindMember(propName);
            if (member != null) {
                member.SetValue(obj, value);
                return;
            }
            //obj的类型的属性是否已经注册
            PropertyDescriptorCollection pdc = obj.GetPropertyDescriptorCollection();
            if (pdc == null)
                RegisteProperties(obj);
            pdc = obj.GetPropertyDescriptorCollection();
            //没有可以注册的属性
            if (pdc == null)
            {
                if (throwException) throw new Exception("类型" + obj.GetType().Name + "没有可以注册的属性 ");
                else return;
            }
            PropertyDescriptor pd = pdc.Get(propName);
            int index = pdc.IndexOf(propName);
            if (index < 0)//属性名不存在
            {
                if (throwException) throw new Exception("属性名不存在:" + propName);
                else return;
            }
            if (!objectPropertiesValues.ContainsKey(obj))
                objectPropertiesValues[obj] = new Dictionary<PropertyDescriptor, object>();
            Dictionary<PropertyDescriptor, object> values = objectPropertiesValues[obj];
            values.SetValue(pd, value);            
        }
        /// <summary>
        /// 修改缺省值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetDefaultValue(this IPropertyObject obj, Object value,bool throwException)
        {
            PropertyDescriptorCollection pdc = obj.GetPropertyDescriptorCollection();
            if (pdc == null)
                RegisteProperties(obj);
            pdc = obj.GetPropertyDescriptorCollection();
            //没有可以注册的属性
            if (pdc == null)
            {
                if (throwException) throw new Exception("类型" + obj.GetType().Name + "没有可以注册的属性 ");
                else return;
            }
            PropertyDescriptor pd = pdc.GetDefault();
            if (pd == null)
            {
                if (throwException) throw new Exception("类型" + obj.GetType().Name + "没有缺省属性");
                else return;
            }
            int index = pdc.IndexOf(pd);
            if (index < 0) return;

            if (!objectPropertiesValues.ContainsKey(obj))
                objectPropertiesValues[obj] =new Dictionary<PropertyDescriptor, object>();
            Dictionary<PropertyDescriptor, object> values = objectPropertiesValues[obj];
            values.SetValue(pd,value);
        }
        #endregion

        #region 属性注册

        /// <summary>
        /// 注册类属性
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="pds"></param>
        public static void RegisteProperties(this IPropertyObject obj,PropertyDescriptorCollection pds=null)
        {
            if (typeProperties.ContainsKey(obj.GetType()))
                return;
            if (pds == null || pds.Count<=0)
            {
                PropertiesAttribute attr = obj.GetType().FindAttribute<PropertiesAttribute>(false);
                if (attr == null || attr.items == null || attr.items.Length <= 0) return;
                pds = new PropertyDescriptorCollection();
                pds.AddRange(attr.items);
            }

            PropertyDescriptorCollection pdc = new PropertyDescriptorCollection();
            pdc.AddRange(pds);
            typeProperties[obj.GetType()] = pdc;
            if (!objectPropertiesValues.ContainsKey(obj))
                objectPropertiesValues[obj] = new Dictionary<PropertyDescriptor, object>();
        }
        /// <summary>
        /// 注册属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pds"></param>
        public static void RegisteProperties<T>(PropertyDescriptorCollection pds) //where T : IPropertyObject
        {
            if (typeProperties.ContainsKey(typeof(T)))
                return;
            if (pds == null || pds.Count <= 0)
            {
                PropertiesAttribute attr = typeof(T).FindAttribute<PropertiesAttribute>(false);
                if (attr == null || attr.items == null || attr.items.Length <= 0) return;
                pds = new PropertyDescriptorCollection();
                pds.AddRange(attr.items);
            }

            
            typeProperties[typeof(T)] = pds;
        }

        
        #endregion

        #region 字符串处理
        /// <summary>
        /// 转换字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static String Format(this IPropertyObject obj,String sep=",")
        {
            PropertyDescriptorCollection pdc = obj.GetPropertyDescriptorCollection();
            if (pdc == null)
                return "";
            Comparison<PropertyDescriptor> comparison = (x, y) => x.xh - y.xh;
            pdc.Sort(comparison);

            StringBuilder str = new StringBuilder();
            foreach(PropertyDescriptor pd in pdc)
            {
                if (str.ToString() != "")
                    str.Append(sep);
                Object value = obj.GetValue<Object>(pd.name);
                str.Append(ConvertUtils.ConvertTo<String>(value, pd.format));
            }
            return str.ToString();
            
        }

        /// <summary>
        /// 解析字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T Parse<T>(String str, String sep = ",",String[] columns=null) where T : IPropertyObject
        {
            if (str == null || str == "") return default(T);
            PropertyDescriptorCollection pdc = GetPropertyDescriptorCollection(typeof(T));
            if (pdc == null)
                return default(T);
            Comparison<PropertyDescriptor> comparison = (x, y) => x.xh - y.xh;
            pdc.Sort(comparison);
            List<PropertyDescriptor> pdList = new List<PropertyDescriptor>();
            if(columns != null && columns.Length > 0)
            {
                for(int i=0;i<columns.Length;i++)
                {
                    PropertyDescriptor pd = pdc.FirstOrDefault<PropertyDescriptor>(x => x.hasName(columns[i]));
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
            for(int i=0;i<ss.Length;i++)
            {                
                Object value = ConvertUtils.ConvertTo(ss[i], pdList[i].Type);
                instance[pdList[i].Name] = value;
            }
            return instance;
        }
        #endregion
    }
    

}
