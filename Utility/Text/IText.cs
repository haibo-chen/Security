using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.RegularExpressions;

using insp.Utility.Common;
using insp.Utility.Bean;
using insp.Utility.Reflection;


namespace insp.Utility.Text
{
    public interface IText
    {
        

             
    }

    public static class TextUtils
    {
        /// <summary>
        /// 按照属性值格式格式化
        /// </summary>
        public const String FMT_PV = "{$P}={$V}";
        public const String FMT_V = "{$V}";

        
        /// <summary>
        /// 对象转文本
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="format"></param>
        /// <param name="seq"></param>
        /// <returns></returns>
        public static String ToText(this Object obj, String format = FMT_PV, String seq = ",")
        {
            if (format == null || format == "") format = FMT_PV;
            if (seq == null || seq == "") seq = ",";

            MemberInfo[] memberInfos = obj.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);            

            List<String> results = new List<string>();

            for (int i = 0; i < memberInfos.Length; i++)
            {                
                TransinetAttribute transinet = memberInfos[i].GetCustomAttribute<TransinetAttribute>();
                if (transinet == null) continue;
                TextAttribute txtAttr = memberInfos[i].GetCustomAttribute<TextAttribute>();
                if (txtAttr != null && txtAttr.Ignored) continue;
                String name = memberInfos[i].Name;
                if (txtAttr != null && StringUtils.NotEmpty(txtAttr.ShowText))
                    name = txtAttr.ShowText;
                Object value = memberInfos[i].FindValue(obj);
                String valueStr = value == null ? "" : ConvertUtils.ConvertTo<String>(value, txtAttr != null ? txtAttr.Format : "");
                String text = format;
                text = text.Replace("{$P}", name);
                text = text.Replace("{$V}", valueStr);
                results.Add(text);
            }
            return results.Aggregate<String>((x, y) => x + seq + y);

        }
        /// <summary>
        /// 寻找第一个出现的整数
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="numberpos">两项数组，分别是整数出现位置</param>
        /// <returns>整数值</returns>
        public static int FeatchNumber(String str, int[] numberpos)
        {
            if (numberpos == null)
                numberpos = new int[2];
            numberpos[0] = numberpos[1] = -1;
            String s = "";
            for (int i=0;i<str.Length;i++)
            {
                if(str[i] >= '0' && str[i] <= '9')
                {
                    if (numberpos[0] == -1)
                    {
                        numberpos[0] = i;
                    }
                    s += str[i]; 
                }
                else
                {
                    if(numberpos[0] != -1)
                    {
                        numberpos[1] = i-1;
                        return int.Parse(s);
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// 对象转文本
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="format"></param>
        /// <param name="seq"></param>
        /// <returns></returns>
        /*public static String ToText(this Object obj,String format= FMT_PV, String seq = ",")
        {
            if (format == null || format == "") format = FMT_PV;
            if (seq == null || seq == "") seq = ",";

            
            PropertyInfo[] propInfos = obj.GetType().GetProperties();

            List<String> results = new List<string>();

            for(int i=0;i< propInfos.Length;i++)
            {
                TransinetAttribute transinet = propInfos[i].GetCustomAttribute<TransinetAttribute>();
                if (transinet == null) continue;
                TextAttribute txtAttr = propInfos[i].GetCustomAttribute<TextAttribute>();
                if (txtAttr != null && txtAttr.Ignored) continue;
                String name = propInfos[i].Name;
                if (txtAttr != null && StringUtils.NotEmpty(txtAttr.ShowText))
                    name = txtAttr.ShowText;                
                Object value = propInfos[i].GetValue(obj);
                String valueStr = value == null ? "" : ConvertUtils.ConvertTo<String>(value, txtAttr != null ? txtAttr.Format : "");
                String text = format;
                text = text.Replace("{$P}", name);
                text = text.Replace("{$V}", valueStr);
                results.Add(text);
            }
            return results.Aggregate<String>((x, y) => x + seq + y);

        }*/


    }
}
