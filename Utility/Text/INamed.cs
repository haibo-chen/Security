using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using insp.Utility.Reflection;

namespace insp.Utility.Text
{
    /// <summary>
    /// 命名对象
    /// 继承该接口的类必须定义(公有私有不限)
    /// String name
    /// String caption
    /// List<String> alias
    /// </summary>
    public interface INamed
    {
    }

    public static class INamedHelper
    {
        public static String getName(this INamed named)
        {
            if (named == null) return "";
            if(named.GetType().FindMember("name") != null)
                return named.FindMemberValue<String>("name");
            TextAttribute txtAttr = named.GetType().GetCustomAttribute<TextAttribute>();
            if (txtAttr == null)
                return "";
            return txtAttr.Name;
        }
        public static String getCaption(this INamed named)
        {
            if (named == null) return "";
            if (named.GetType().FindMember("caption") != null)
                return named.FindMemberValue<String>("caption");
            TextAttribute txtAttr = named.GetType().GetCustomAttribute<TextAttribute>();
            if (txtAttr == null)
                return "";
            return txtAttr.Caption;
        }
        public static List<String> getAlias(this INamed named)
        {
            if (named == null) return null;
            if (named.GetType().FindMember("alias") != null)
                return new List<string>(named.FindMemberValue<List<String>>("alias"));
            TextAttribute txtAttr = named.GetType().GetCustomAttribute<TextAttribute>();
            if (txtAttr == null || txtAttr.Alias==null)
                return null;
            return new List<string>(txtAttr.Alias);
        }
        public static List<String> GetNames(this INamed named)
        {
            List<String> r = new List<string>();
            String v = named.getName();
            if (v != null && v != "")
                r.Add(v);
            v = named.getCaption();
            if (v != null && v != "")
                r.Add(v);
            List<String> alias = named.getAlias();
            if (alias != null && alias.Count > 0)
                r.AddRange(alias);
            return r;
        }

        /// <summary>
        /// 名称是否匹配
        /// </summary>
        /// <param name="name">待匹配名称</param>
        /// <param name="ignoreCase">大小写敏感</param>
        /// <param name="includename">名称匹配</param>
        /// <param name="includeCaption">名称匹配</param>
        /// <param name="includeAlias">别名匹配</param>
        /// <returns></returns>
        public static bool hasName(this INamed named,String name, bool ignoreCase = true, bool includename = true, bool includeCaption = true, bool includeAlias = true)
        {
            if (name == null || name == "") return false;
            StringComparison sc = ignoreCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
            if (includename)
                if (name.Equals(named.getName(), sc))
                    return true;
            if (includeCaption)
                if (name.Equals(named.getCaption(), sc))
                    return true;
            if (!includeAlias)
                return false;
            List<String> alias = named.getAlias();
            foreach (String alia in alias)
            {
                if (name.Equals(alia, sc))
                    return true;
            }
            return false;
        }
    }
}
