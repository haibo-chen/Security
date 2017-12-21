using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Bean;

namespace insp.Utility.Collections
{
    public interface IListOperator
    {

    }
    public interface IListOperator<T> : IListOperator
    {

    }
    public static class CollectionUtils
    {
        #region List操作
        public static IList ListGet(this IListOperator list, String name="")
        {
            if (name == null || name == "") name = "elements";
            return Reflection.ReflectionUtils.FindMemberValue<IList>(list, name);
        }
        public static List<T> ListGet<T>(this IListOperator list,String name="")
        {
            if (name == null || name == "") name = "elements";
            return  Reflection.ReflectionUtils.FindMemberValue<List<T>>(list, name);
        }

        public static T ListIndexGet<T>(this IListOperator list, int index,String name="")
        {
            if (name == null || name == "") name = "elements";
            List<T> vs = Reflection.ReflectionUtils.FindMemberValue<List<T>>(list, name);
            return vs == null || vs.Count >= index ? default(T) : vs[index];
        }

        public static T ListIndexSet<T>(this IListOperator list, int index,T t, String name="")
        {
            if (name == null || name == "") name = "elements";
            List<T> vs = Reflection.ReflectionUtils.FindMemberValue<List<T>>(list, name);
            T old = vs == null || vs.Count >= index ? default(T) : vs[index];
            if (index < vs.Count)
                vs[index] = t;
            return old;
        }
        public static void ListIndexAdd<T>(this IListOperator list, T t, String name="")
        {
            if (name == null || name == "") name = "elements";
            List<T> vs = Reflection.ReflectionUtils.FindMemberValue<List<T>>(list, name);
            vs.Add(t);
        }

        public static T ListIndexSetOrAdd<T>(this IListOperator list, T t, String name="")
        {
            if (name == null || name == "") name = "elements";
            List<T> vs = Reflection.ReflectionUtils.FindMemberValue<List<T>>(list, name);
            if (vs == null) return default(T);
            int index = vs.IndexOf(t);
            T old = default(T);
            if (index >= 0) { old = vs[index]; vs[index] = t; }
            else vs.Add(t);
            return old;
        }
        #endregion

        #region 集合创建
        /// <summary>
        /// 数组转List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static List<T> AsList<T>(params T[] ts)
        {
            return new List<T>(ts);
        }
        #endregion

        #region 集合相等
        public static Array AsArray(ICollection x)
        {
            if (x == null) return null;
            if (x.Count <= 0) return new Object[0];
            Object[] r = new Object[x.Count];

            IEnumerator tor = x.GetEnumerator();
            int i = 0;
            while(tor.MoveNext())
            {
                r[i++] = tor.Current;
            }
            return r;
        }
        /// <summary>
        /// 比较两个数组
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="partialMatch"></param>
        /// <returns></returns>
        public static bool Equals(this Array a, Array b, bool partialMatch = false)
        {
            if (a == null && b == null) return true;
            else if (a == null || b == null) return false;

            
            if (!partialMatch && a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (i >= b.Length)
                    return partialMatch;
                Object av = a.GetValue(i);
                Object bv = b.GetValue(i);
                if (av== null &&  bv != null) return false;
                else if (av != null && bv == null) return false;
                
                if (!av.Equals(bv)) return false;
            }
            return true;
        }
        public static bool Equals(this Array a, ICollection b)
        {
            if (a == null && b == null) return true;
            else if (a == null || b == null) return false;

            return Equals(a, AsArray(b));
            
        }
        /// <summary>
        /// 比较两个List
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="partialMatch"></param>
        /// <returns></returns>
        public static bool Equals(this ICollection a, ICollection b,bool partialMatch=false)
        {
            if (a == null && b == null) return true;
            else if (a == null || b == null) return false;

            if (a == null && b == null) return true;
            else if (a == null || b == null) return false;

            return Equals(AsArray(a), AsArray(b));
        }

        /// <summary>
        /// 比较两个List
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="partialMatch"></param>
        /// <returns></returns>
        public static bool Equals(this ICollection a, Array b, bool partialMatch = false)
        {
            if (a == null && b == null) return true;
            else if (a == null || b == null) return false;

            if (a == null && b == null) return true;
            else if (a == null || b == null) return false;

            return Equals(AsArray(a), b);
        }
        #endregion

        #region 字典
        public static Dictionary<K, V> AsDictionary<K, V>(params Object[] kvpairs)
        {
            Dictionary<K, V> dict = new Dictionary<K, V>();
            for(int i=0;i<kvpairs.Length-1;i+=2)
            {                
                dict.Add((K)kvpairs[i], (V)kvpairs[i + 1]);
            }
            return dict;
        }
        /// <summary>
        /// 取值
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static V GetValue<K,V>(this Dictionary<K,V> dict,K key,V defaultValue=default(V))
        {
            if (dict.ContainsKey(key))
                return dict[key];
            return defaultValue;
        }

        public static V GetValue<K,V>(this ConcurrentDictionary<K,V> dict,K key, V defaultValue = default(V))
        {
            if (dict.ContainsKey(key))
                return dict[key];
            return defaultValue;
        }
        /// <summary>
        /// 添加或修改值
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static V SetValue<K,V>(this Dictionary<K, V> dict,K key,V value)
        {
            V r = default(V);
            if(dict.ContainsKey(key))
            {
                r = dict[key];
                dict[key] = value;
            }
            else
            {
                dict.Add(key,value);
            }
            return r;
        }

        /// <summary>
        /// 添加或修改值
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static V SetValue<K, V>(this ConcurrentDictionary<K, V> dict, K key, V value)
        {
            V r = default(V);
            if (dict.ContainsKey(key))
            {
                r = dict[key];
                dict[key] = value;
            }
            else
            {
                dict[key] = value;
            }
            return r;
        }
        #endregion

        #region 排列组合
        /// <summary>
        /// 组合
        /// 例如传入:[1,2] [a,b]  [x,y,z]
        /// 返回[1,a,x] [1,a,y] [1,a,z] [1,b,x] [1,b,y] [1,b,z] [2,a,x] [2,a,y] [2,a,z] [2,b,x] [2,b,y] [2,b,z]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arrays"></param>
        /// <returns></returns>
        public static List<T>[] Combination<T>(params List<T>[] arrays)
        {
            if (arrays.Length == 0)
                return null;
            else if (arrays.Length == 1)
                return new List<T>[] { arrays[0] };
            else if (arrays.Length == 2)
                return Combination2(arrays[0], arrays[1]);

            List<List<T>> results = new List<List<T>>();
            List<T>[] r1 = Combination2(arrays[0], arrays[1]);
            for(int i=2;i< arrays.Length;i++)
            {
                r1 = Combination2(r1,arrays[i]);
                
            }

            return r1;
        }

        public static List<T>[] Combination2<T>(List<T>[] a1, List<T> a2)
        {
            List<List<T>> r = new List<List<T>>();
            for (int i=0;i<a2.Count;i++)
            {
                for(int j=0;j<a1.Length;j++)
                {
                    List<T> temp = new List<T>(a1[j]);
                    temp.Add(a2[i]);
                    r.Add(temp);
                }
            }
            return r.ToArray();
        }
        /// <summary>
        /// 组合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="results"></param>
        public static List<T>[] Combination2<T>(List<T> a1,List<T> a2)
        {            
            List<T>[] results = new List<T>[a1.Count*a2.Count];
            int num = 0;
            for(int i=0;i<a1.Count;i++)
            {
                for(int j=0;j<a2.Count;j++)
                {
                    results[num] = new List<T>();
                    results[num].Add(a1[i]);
                    results[num].Add(a2[j]);
                    num++;
                }
            }
            return results;
        }
        #endregion

        #region 类型转换
        /// <summary>
        /// List转换
        /// </summary>
        /// <typeparam name="K1"></typeparam>
        /// <typeparam name="K2"></typeparam>
        /// <param name="values"></param>
        /// <param name="format"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public static List<K2> ToList<K1, K2>(this List<K1> values, String format = "", Properties props = null)
        {
            return ConvertUtils.listtolist<K1, K2>(values, format, props);
        }
        /// <summary>
        /// 转换为字符串 
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="values"></param>
        /// <param name="format"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public static String ToString<K>(this List<K> values, String format = "", Properties props = null)
        {
            return ConvertUtils.listTostr<K>(values, format, props);
        }
        /// <summary>
        /// 转换为字符串 
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="values"></param>
        /// <param name="format"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public static String ToString<K>(this K[] values, String format = "", String sep=",")
        {
            Properties prop = new Properties();
            prop.Put("sep", sep);
            return ConvertUtils.arraytostr<K>(values, format, prop);
        }

        #endregion

        #region 文件读写
        public static void Save<T>(this List<T> list,String filename,String encode,String[] columns,String format)
        {

        }
        #endregion
    }

    public static class CollectionSLOPEUtils
    {
        
        public static double SLOPE(this List<double> list)
        {
            int num = list.Count;
            double avgx = (num - 1) / 2;
            double avgy = list.Average();

            double b1 = 0, b2 = 0;
            for (int i = 0; i < list.Count; i++)
            {
                b1 += (i - avgx) * (list[i] - avgy);
                b2 += (i - avgx) * (i - avgx);
            }
            return b1 / b2;
        }
    }
}
