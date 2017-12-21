using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Collections;
using insp.Utility.Bean;

namespace insp.Utility.Date
{
    
    public static class DateUtils
    {
        #region 常用格式
        public static readonly String FMT_DATETIME_DEFAULT = "yyyy-MM-dd hh:mm:ss";
        public static readonly String FMT_DATETIME_COMPACT = "yyyyMMddhhmmss";
        public static readonly String FMT_DATETIME_NORMAL = "yyyy/MM/dd hh:mm:ss";

        public static readonly String FMT_DATE_DEFAULT = "yyyy-MM-dd";
        public static readonly String FMT_DATE_COMPACT = "yyyyMMdd";
        public static readonly String FMT_DATE_NORMAL = "yyyy/MM/dd";

        public static readonly String[] FMT_DATETIME = { FMT_DATETIME_DEFAULT, FMT_DATETIME_COMPACT };
        public static readonly String[] FMT_DATE = { FMT_DATE_DEFAULT, FMT_DATE_COMPACT, FMT_DATE_NORMAL };
        
        private static readonly List<String> registeredFormats = new List<String>(FMT_DATETIME.Concat(FMT_DATE));
        public static String[] FORMATS
        {
            get { return registeredFormats.ToArray(); }
        }

        /// <summary>
        /// 注册新格式
        /// </summary>
        /// <param name="fmts"></param>
        /// <returns></returns>
        public static String[] RegisteFormat(params String[] fmts)
        {
            registeredFormats.AddRange(fmts);
            return registeredFormats.ToArray();
        }

        public static readonly DateTime InitDate = new DateTime(1970, 1, 1);
        #endregion

        #region 解析
        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="str"></param>
        /// <param name="format"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static bool TryParse(String str, out DateTime d)
        {
            
            d = InitDate;
            foreach (String ftm in registeredFormats)
            {
                if (DateTime.TryParseExact(str, ftm, null, System.Globalization.DateTimeStyles.None, out d))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultFormat"></param>
        /// <returns></returns>
        public static DateTime Parse(String str,String defaultFormat="")
        {
            DateTime d;
            if (DateTime.TryParseExact(str, defaultFormat,null, System.Globalization.DateTimeStyles.None, out d))
                return d;
            foreach(String ftm in registeredFormats)
            {
                if (DateTime.TryParseExact(str, ftm, null, System.Globalization.DateTimeStyles.None, out d))
                    return d;
            }
            return DateTime.ParseExact(str, defaultFormat,null);
        }
        #endregion

        #region 转字符串
        public static String ToText(this DateTime d,String format="",Properties context=null)
        {
            if (format == null || format == "")
                return d.ToString(FMT_DATETIME_DEFAULT);
            else
                return d.ToString(format);
        }
        #endregion
    }
}
