using insp.Utility.Bean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Utility.Text
{
    public static class StringUtils
    {
        #region 基本操作
        
        public static bool NotEmpty(this String str)
        {
            return str != null && str != "";
        }
        #endregion

        #region 子串
        public static String substring(this String str,int begin,String beginStr,String endStr)
        {
            if (str == null || str == "") return str;
            int bpos = str.IndexOf(beginStr, begin);
            if (bpos < 0) return null;
            int epos = str.IndexOf(endStr, bpos);
            if (epos < 0) return null;

            return str.Substring(bpos + beginStr.Length, epos - bpos - beginStr.Length);
        }
        /// <summary>
        /// 按照seps分裂，双引号中间的seps部分会当普通字符
        /// </summary>
        /// <param name="str"></param>
        /// <param name="seps"></param>
        /// <returns></returns>
        public static String[] Spilt2(this String str,params char[] seps)
        {
            List<String> r = new List<string>();
            String cur = "";
            bool ignore = false;
            for(int i=0;i<str.Length;i++)
            {
                if(seps.Contains(str[i]))
                {
                    if (ignore)
                    {
                        cur += str[i];
                        continue;
                    }
                    r.Add(cur);
                    cur = "";
                    continue;
                }
                cur += str[i];
                if (str[i] == '"')
                    ignore = !ignore;
            }
            r.Add(cur);
            return r.ToArray();
        }


        #endregion

        #region 转换
        public static String FromDoubleArray(this double[] values,String format="",String sep=",")
        {
            if (values == null||values.Length<=0) return "";
            return new List<double>(values).ConvertAll(x => x.ToString(format)).Aggregate((x, y) => x + sep + y);
        }
        /// <summary>
        /// 字符串数组转换double数组 
        /// </summary>
        /// <param name="strs"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static double[] ToDoubleArray(this String[] strs,int start=0)
        {
            if (strs == null || strs.Length <= start)
                return null;
            List<double> ds = new List<double>();
            for(int i=start;i<strs.Length;i++)
            {
                double val = 0;
                if (double.TryParse(strs[i], out val))
                    ds.Add(val);
            }
            return ds.ToArray();
        }
        #endregion


        #region 数字处理
        public static bool IsDigital(this char x)
        {
            return x >= '0' && x <= '9';
        }


        #endregion

        #region 字符串集合
        public static void Trim(this String[] strs)
        {
            if (strs == null) return;
            for (int i = 0; i < strs.Length; i++)
                strs[i] = strs[i] == null ? "" : strs[i].Trim();
        }

        public static String Merge(this String[] strs,int start=0,int end=0,String sep=",")
        {
            if (strs == null || strs.Length <= 0) return "";
            if (start < 0) start = 0;
            if (end < 0 || end >= strs.Length)
                end = strs.Length - 1;
            StringBuilder s = new StringBuilder();
            for(int i=start;i<=end;i++)
            {
                if (s.ToString() != "")
                    s.Append(sep);
                s.Append(strs[i]);
            }
            return s.ToString();
        }
        public static bool Contain(this String[] strs,String s,bool ignoreCase)
        {
            if (strs == null || strs.Length <= 0) return false;
            return strs.ToList().Exists(x => x.Equals(s, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture));
        }
        
        #endregion
    }

    
}
