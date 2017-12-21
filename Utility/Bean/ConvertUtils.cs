using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using System.Reflection;

using insp.Utility.Common;
using insp.Utility.Date;
using insp.Utility.Text;
using insp.Utility.Reflection;


namespace insp.Utility.Bean
{
    /// <summary>
    /// 转换器方法
    /// </summary>
    /// <typeparam name="Tin">输入类型</typeparam>
    /// <typeparam name="Tout">输出类型</typeparam>
    /// <param name="src">源对象</param>
    /// <param name="format">转换格式</param>
    /// <param name="props">转换参数</param>
    /// <returns></returns>
    public delegate Tout DoConvertor<in Tin, out Tout>(Tin src, String format = "", Properties context = null);    

    
    /// <summary>
    /// 转换工具
    /// </summary>
    public static class ConvertUtils
    {
        #region 转换器管理
        /// <summary>
        /// 转换器
        /// </summary>
        private static readonly Dictionary<KeyValuePair<Type, Type>, Delegate> converters = new Dictionary<KeyValuePair<Type, Type>, Delegate>();
        /// <summary>
        /// 缺省转换器
        /// </summary>
        private static readonly Dictionary<KeyValuePair<Type, Type>, Delegate> defaultconverters = new Dictionary<KeyValuePair<Type, Type>, Delegate>();

        /// <summary>
        /// 转换器注册
        /// </summary>
        /// <typeparam name="Tin"></typeparam>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="converter"></param>
        public static void RegisteConvertor<Tin, Tout>(DoConvertor<Tin, Tout> converter)
        {
            
            KeyValuePair<KeyValuePair<Type, Type>, Delegate>  r = converters.FirstOrDefault<KeyValuePair<KeyValuePair<Type, Type>, Delegate>>(kvp=>kvp.Key.Key==typeof(Tin)&&kvp.Key.Key==typeof(Tout));
            if(r.Value != null)
            {
                converters[r.Key] = converter;
                return;
            }
            KeyValuePair<Type, Type> key = new KeyValuePair<Type, Type>(typeof(Tin),typeof(Tout));
            converters.Add(key,converter);
          
        }

        
        /// <summary>
        /// 取得转换器
        /// </summary>
        /// <typeparam name="Tin"></typeparam>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="includeDefault"></param>
        /// <returns></returns>
        public static DoConvertor<Tin, Tout> GetConvert<Tin, Tout>(bool includeDefault=true)
        {
            DoConvertor<Tin, Tout>  conv = doGetConvert<Tin, Tout>(converters, false, false);
            if (conv != null)
                return conv;
            if (!includeDefault)
                return null;

            conv = doGetConvert<Tin, Tout>(defaultconverters, false, false);
            if (conv != null)
                return conv;
            
            return null;
        }

        private static DoConvertor<Tin, Tout> doGetConvert<Tin, Tout>(Dictionary<KeyValuePair<Type, Type>, Delegate> convertors,bool srcAllowChild,bool destAllowChild)
        {
            if (convertors == null)
                return null;
            //先尝试完全匹配
            foreach (KeyValuePair<Type, Type> typePair in converters.Keys)
            {
                if (typePair.Key == typeof(Tin) && typePair.Value == typeof(Tout))
                    return (DoConvertor<Tin, Tout>)converters[typePair];                
            }
            //匹配子类
            foreach (KeyValuePair<Type, Type> typePair in converters.Keys)
            {
                if (typePair.Key == typeof(Tin) && typePair.Value == typeof(Tout))
                    return (DoConvertor<Tin, Tout>)converters[typePair];

                if (!srcAllowChild && typePair.Key != typeof(Tin))
                    continue;
                if (srcAllowChild && !typePair.Key.IsAssignableFrom(typeof(Tin)))
                    continue;

                if (!destAllowChild && typePair.Value != typeof(Tout))
                    continue;
                if (destAllowChild && !typePair.Value.IsAssignableFrom(typeof(Tout)))
                    continue;

                return (DoConvertor<Tin, Tout>)converters[typePair];
            }
            return null;
                


        }
        #endregion

        #region 转换方法 
        /// <summary>
        /// 执行转换
        /// </summary>
        /// <typeparam name="Tin"></typeparam>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="src"></param>
        /// <param name="format"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool TryConvertTo<Tin, Tout>(this Tin src, out Tout r, String format = "",Properties context=null)
        {
            r = default(Tout);
            try
            {
                r = ConvertTo<Tin, Tout>(src, format,context);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// 执行转换
        /// </summary>
        /// <typeparam name="Tin"></typeparam>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="src"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static Tout ConvertTo<Tin,Tout>(this Tin src,String format="", Properties context = null)
        {
            if (typeof(Tin) == typeof(Tout))
                return (Tout)(Object)src;
            if (typeof(Tout) == typeof(Object))
                return (Tout)(Object)src;

            DoConvertor<Tin, Tout> convert = GetConvert<Tin, Tout>();
            if (convert != null)
                return convert(src, format,context);
            
            if (src.Equals(default(Tin)))
                return default(Tout);

            if (typeof(Tout) == typeof(String))//如果目标类型是字符串
            {
                if (typeof(Tin).GetMethod("ToString").DeclaringType == typeof(Tin))
                    return (Tout)(Object)src.ToString();//如果源类型实现了ToString，则直接调用
                
                return (Tout)(Object)objectToStr(src, format, context);
            }
            else if (typeof(Tin) == typeof(String))
            {
                MethodInfo method = typeof(Tout).GetMethod("Parse", BindingFlags.Static);
                if (method != null)//如果目标类型实现静态方法Ｐａｒｓｅ，则直接调用　
                    return (Tout)method.Invoke(null, new Object[] { src });
                return (Tout)strToObject(src.ToString(), typeof(Tout), format, context);
            }
            
            throw new NotImplementedException();
        }

        /// <summary>
        /// 执行转换
        /// </summary>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="src"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static bool TryConvertTo<Tout>(Object src, out Tout r, String format = "", Properties context = null)
        {
            r = default(Tout);
            try
            {
                r = ConvertTo<Tout>(src, format,context);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }

        }

        /// <summary>
        /// 执行转换
        /// </summary>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="src"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static Tout ConvertTo<Tout>(Object src, String format="", Properties context = null)
        {
            MethodInfo method = typeof(ConvertUtils).FindGenericMethod("ConvertTo", new Type[] { src.GetType(), typeof(String),typeof(Properties)},src.GetType(), typeof(Tout));
            if (method == null) return default(Tout);
            return (Tout)method.Invoke(null, new Object[] { src, format,context });
            
        }
        /// <summary>
        /// 执行转换
        /// </summary>
        /// <param name="src"></param>
        /// <param name="destType"></param>
        /// <param name="format"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool TryConvertTo(Object src, Type destType, out Object r, String format = "", Properties context = null)
        {
            r = null;
            try
            {
                r = ConvertTo(src, destType, format,context);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }
        /// <summary>
        /// 执行转换
        /// </summary>
        /// <param name="src"></param>
        /// <param name="destType"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static Object ConvertTo(Object src,Type destType,String format="", Properties context = null)
        {
            if (src != null && src.GetType() == typeof(String))
                return strToObject(src.ToString(), destType, format, context);
            if (destType != null && destType == typeof(String))
                return objectToStr(src, format, context);

            MethodInfo method = typeof(ConvertUtils).FindGenericMethod("ConvertTo", new Type[] {src.GetType(),typeof(String),typeof(Properties) },src.GetType(), destType);
            if (method == null) return null;
            return method.Invoke(null, new Object[] { src, format,context });
        }

        #endregion

        #region 缺省转换器实现
        /// <summary>
        /// 静态初始化，注册缺省转换器
        /// </summary>
        static ConvertUtils()
        {
            RegisteConvertor<int, String>(intToStr);
            RegisteConvertor<String,int>(strToInt);
            RegisteConvertor<double, String>(doubleToStr);
            RegisteConvertor<String, double>(strTodouble);
            RegisteConvertor<DateTime, String>(datetimeToStr);
            RegisteConvertor<String, DateTime>(strToDatetime);
            RegisteConvertor<Object, String>(objectToStr);
            RegisteConvertor<String, List<double>>(strtolist<double>);
            RegisteConvertor<String, char>(strtochar);
            RegisteConvertor<String, bool>(strtobool);
            RegisteConvertor<bool, String>(booltostr);
        }


        
        #region 缺省字符串和枚举类型
        public static String enumtostr<K>(K x, String format = "", Properties prop = null)
        {
            if (x == null) return "";
            return x.ToString();
        }
        public static K strtoenum<K>(String x, String format = "", Properties prop = null)
        {
            return (K)Enum.Parse(typeof(K), x);
        }
        #endregion

        #region 缺省布尔字符串转换
        public static bool strtobool(String x, String format = "", Properties prop = null)
        {
            if (x == null) return false;
            if (x == "0") return false;

            if (x.ToLower() == "true")
                return true;
            return false;
        }
        public static String booltostr(bool x, String format = "", Properties prop = null)
        {
            return x.ToString();
        }
        #endregion

        #region 缺省整数字符串转换器
        public static char strtochar(String x, String format = "", Properties prop = null)
        {
            return x == null || x == "" ? ' ' : x[0];
        }

        /// <summary>
        /// 字符串转int
        /// </summary>
        /// <param name="x"></param>
        /// <param name="format"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static int strToInt(String x, String format = "",Properties prop = null)
        {
            try
            {
                int r = int.Parse(x, System.Globalization.NumberStyles.Any, null);
                return r;
            }catch(Exception e)
            {
                throw e;
            }
            
        }
        /// <summary>
        /// 整数转字符串
        /// </summary>
        /// <param name="x"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private static String intToStr(int x,String format="", Properties prop = null)
        {
            if (format == null || format == "")
                return x.ToString();
            if (format.Equals("bin", StringComparison.InvariantCultureIgnoreCase) ||
               format.Equals("binary", StringComparison.InvariantCultureIgnoreCase) ||
               format.Equals("2", StringComparison.InvariantCultureIgnoreCase))
                return System.Convert.ToString(x, 2);
            else if (format.Equals("oct", StringComparison.InvariantCultureIgnoreCase) ||
                    format.Equals("octal", StringComparison.InvariantCultureIgnoreCase) ||
                    format.Equals("octonary", StringComparison.InvariantCultureIgnoreCase) ||
                    format.Equals("8", StringComparison.InvariantCultureIgnoreCase))
                return System.Convert.ToString(x, 8);
            else if (format.Equals("hex", StringComparison.InvariantCultureIgnoreCase) ||
                    format.Equals("hexadecimal ", StringComparison.InvariantCultureIgnoreCase) ||
                    format.Equals("16", StringComparison.InvariantCultureIgnoreCase))
                return System.Convert.ToString(x, 8);
            else
                return x.ToString();
        }
        #endregion

        #region 缺省浮点数字符串转换
        private static String doubleToStr(double x, String format = "",Properties prop=null)
        {
            return x.ToString(format);
        }
        private static double strTodouble(String x,String format="", Properties prop = null)
        {
            return (double)Convert.ChangeType(x, typeof(double));
        }
        #endregion

        #region 缺省日期时间转换字符串转换

        private static String datetimeToStr(DateTime x,String format="", Properties prop = null)
        {
            if (format == null || format == "")
                format = Date.DateUtils.FMT_DATETIME_DEFAULT;
            return x.ToString(format);
        }
        private static DateTime strToDatetime(String x,String format="",Properties prop=null)
        {
            DateTime t = DateTime.Now;

            String[] formats = Date.DateUtils.FORMATS;
            for (int i=0;i< formats.Length;i++)
            {
                if (DateTime.TryParseExact(x, formats[i], null, System.Globalization.DateTimeStyles.None, out t))
                    return t;
            }
            throw new Exception("字符串转日期时间类型失败，格式不支持:"+x);
        }
        #endregion

        #region 缺省字符串和对象之间转换
        /// <summary>
        /// 按照属性值格式格式化
        /// </summary>
        public const String TEXT_FMT_PV = "{$P}={$V}";
        public const String TEXT_FMT_CSV = "{$V}";
        public const String TEXT_FMT_DEFAULT = TEXT_FMT_CSV;


        public const String TEXT_PROP_SEP = "separator";
        public const String TEXT_PROP_SEP_DEFAULT = ",";
        public const String TEXT_PROP_FORMAT = "format";
        public const String TEXT_PROP_PROPNAMES = "propertyNames";

        /// <summary>
        /// 对象转字符串
        /// </summary>
        /// <param name="x"></param>
        /// <param name="format"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public static String objectToStr(this Object x,String format="",Properties props=null)
        {
            if (x == null) return "";
            String sep = props == null ? TEXT_PROP_SEP_DEFAULT : props.Get<String>(TEXT_PROP_SEP, TEXT_PROP_SEP_DEFAULT);
            if (format == null || format == "")
                format = props == null ? "" : props.Get<String>(TEXT_PROP_FORMAT,"");
            String[] propertyNames = props == null ? null : props.Get<String[]>(TEXT_PROP_PROPNAMES, null);
            
            //基础数据类型转换
            if (x.GetType() == typeof(char))
                return x.ToString();
            else if (x.GetType() == typeof(int) || x.GetType() == typeof(Int32))
                return intToStr((int)x, format, props);
            else if (x.GetType() == typeof(float) || x.GetType() == typeof(double))
                return doubleToStr((double)x, format, props);
            else if (x.GetType() == typeof(String))
                return (String)x;
            else if (x.GetType() == typeof(DateTime))
                return datetimeToStr((DateTime)x, format, props);
            else if (x.GetType() == typeof(String))
                return x.ToString();
            else if (x.GetType().IsEnum)
                return x.ToString();
            else if (x.GetType().IsArray)
            {
                StringBuilder st = new StringBuilder();
                foreach (Object ele in (Array)x)
                {
                    if (st.ToString() != null) st.Append(",");
                    st.Append(objectToStr(ele, format, props));
                }
                return st.ToString();
            }
            else if (typeof(IEnumerable).IsInstanceOfType(x))
            {
                StringBuilder st = new StringBuilder();
                IEnumerable tor = (IEnumerable)x;
                foreach (Object ele in tor)
                {
                    if (st.ToString() != "") st.Append(",");
                    st.Append(objectToStr(ele, format, props));
                }
                return st.ToString();
            }

            

            MemberInfo[] memberInfos = x.GetType().GetProperties();

            List<String> results = new List<string>();

            for (int i = 0; i < memberInfos.Length; i++)
            {
                TransinetAttribute transinet = memberInfos[i].GetCustomAttribute<TransinetAttribute>();
                if (transinet != null) continue;
                TextAttribute txtAttr = memberInfos[i].GetCustomAttribute<TextAttribute>();
                if (txtAttr != null && txtAttr.Ignored) continue;                
                String name = memberInfos[i].Name;
                if (propertyNames != null && propertyNames.Length > 0 && !propertyNames.Contain(name, true))
                    continue;


                if (txtAttr != null && StringUtils.NotEmpty(txtAttr.ShowText))
                    name = txtAttr.ShowText;
                Object value = memberInfos[i].FindValue(x);
                String valueStr = value == null ? "" : ConvertUtils.ConvertTo<String>(value, txtAttr != null ? txtAttr.Format : "");
                String text = format;
                text = text.Replace("{$P}", name);
                text = text.Replace("{$V}", valueStr);
                results.Add(text);
            }
            return results.Aggregate<String>((a, b) => a + sep + b);
        }

        public static Object strToObject(this String x, Type destType,String format = "", Properties props = null)
        {
            if (x == null || x == "") return null;

            //基础数据类型转换
            if (destType == typeof(char))
                return (Object)x[0];
            else if (destType == typeof(int) || destType == typeof(Int32))
                return (Object)strToInt(x, format, props);
            else if (destType == typeof(float) || destType == typeof(double))
                return (Object)strTodouble(x, format, props);
            else if (destType == typeof(String))
                return (Object)x;
            else if (destType == typeof(DateTime))
                return (Object)strToDatetime(x, format, props);
            else if (destType.IsEnum)
                return Enum.Parse(destType, x);
            else if (destType.IsArray)
            {
                String[] sArray = x.Split(',');
                if (sArray == null || sArray.Length <= 0)
                    return null;
                Type eleType = destType.GetElementType();
                Array inst = Array.CreateInstance(eleType, sArray.Length);
                for (int i = 0; i < sArray.Length; i++)
                {
                    Object t = ReflectionUtils.CallGenericMethod(typeof(ConvertUtils), null, "strToObject", new Type[] { eleType }, new Object[] { sArray[i], format, props });
                    inst.SetValue(t, i);
                }
                return (Object)inst;
            }
            else if (typeof(IEnumerable).IsAssignableFrom(destType))
            {
                String[] sArray = x.Split(',');
                if (sArray == null || sArray.Length <= 0)
                    return null;
                Type eleType = destType.GetGenericArguments()[0];

                Type listType = typeof(List<>);
                listType = listType.MakeGenericType(eleType);

                //IList list = (IList)listType.GetConstructor(null).Invoke(null);
                IList list = (IList)listType.Assembly.CreateInstance(listType.FullName);
                for (int i = 0; i < sArray.Length; i++)
                {
                    Object t = ReflectionUtils.CallGenericMethod(typeof(ConvertUtils), null, "strToObject", new Type[] { eleType }, new Object[] { sArray[i], format, props });
                    list.Add(t);
                }
                return (Object)list;
            }

            if (props == null) props = new Properties();
            String sep = props.Get<String>(TEXT_PROP_SEP, ",");
            if (format == null || format == "")
                format = props.Get<String>(TEXT_PROP_FORMAT, TEXT_FMT_DEFAULT);
            String propNames = props.Get<String>(TEXT_PROP_PROPNAMES, "");

            PropertyInfo[] memberInfos = destType.GetProperties();

            List<PropertyInfo> members = new List<PropertyInfo>(memberInfos);

            String[] ss = x.Split(sep.ToCharArray());
            if (ss == null || ss.Length <= 0) return null;
            Object obj = destType.Assembly.CreateInstance(destType.FullName);


            for (int i = 0; i < ss.Length; i++)
            {
                if (ss[i] == null) ss[i] = "";
                String ttStr = "";
                String tvStr = ss[i].Trim();
                if (ss[i].Contains("="))
                {
                    ttStr = ss[i].Split('=')[0].Trim();
                    tvStr = ss[i].Split('=')[1].Trim();
                }
                PropertyInfo member = members.FirstOrDefault(m => m.HasName(ttStr));
                if (member == null) member = members[i];


                Object value = ConvertTo(ss[i], member.PropertyType);
                if (member.GetSetMethod() != null)
                    member.SetValue(obj, value);
            }
            return obj;
        }
        public static K strToObject<K>(this String x,String format="",Properties props=null)
        {
            return (K)strToObject(x, typeof(K), format, props);
            
        }
        #endregion

        #region 缺省字符串转List
        public static List<T> strtolist<T>(String str,String format="",Properties props = null)
        {
            if (str == null || str == "") return null;
            List<T> results = new List<T>();
            String[] ss = str.Split(',');
            if (ss == null || ss.Length <= 0) return null;
            for(int i=0;i<ss.Length;i++)
            {
                if (ss[i] == null || ss[i] == "" || ss[i].Trim() == "")
                    continue;
                T ele = strToObject<T>(ss[i].Trim());
                results.Add(ele);
            }
            return results;
        }


        #endregion

        #region List之间转换
        
        /// <summary>
        /// List之间转换
        /// </summary>
        /// <typeparam name="K1"></typeparam>
        /// <typeparam name="K2"></typeparam>
        /// <param name="values"></param>
        /// <param name="format"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public static List<K2> listtolist<K1,K2>(List<K1> values, String format = "", Properties props = null)
        {
            if (values == null || values.Count <= 0)
                return null;
            List<K2> k2list = new List<K2>();
            for(int i=0;i<values.Count;i++)
            {
                K1 value = values[i];
                K2 val = ConvertTo<K2>(value, format, props);
                k2list.Add(val);
            }
            return k2list;
        }
        

        /// <summary>
        /// 数组之间转换
        /// </summary>
        /// <typeparam name="K1"></typeparam>
        /// <typeparam name="K2"></typeparam>
        /// <param name="values"></param>
        /// <param name="format"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public static K2[] arraytoarray<K1, K2>(K1[] values, String format = "", Properties props = null)
        {
            List<K2> list = listtolist<K1, K2>(new List<K1>(values), format, props);
            return list == null ? null : list.ToArray();
        }

        /// <summary>
        /// List合并成字符串 
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="values"></param>
        /// <param name="format"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public static String listTostr<K>(List<K> values,String format = "", Properties props = null)
        {
            StringBuilder str = new StringBuilder();
            String sep = ",";
            if (props != null)
                sep = props.Get<String>("sep", ",");
            foreach(K value in values)
            {
                if (str.ToString() != "")
                    str.Append(sep);
                str.Append(ConvertTo<String>(value, format, props));
            }
            return str.ToString();
        }
        /// <summary>
        /// 数组合并成字符串
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="values"></param>
        /// <param name="format"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public static String arraytostr<K>(K[] values, String format = "", Properties props = null)
        {
            return listTostr<K>(new List<K>(values), format, props);
        }
        #endregion
        #endregion
    }


}
