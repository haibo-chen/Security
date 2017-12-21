using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Utility.Bean
{
    /// <summary>
    /// 属性描述符
    /// </summary>
    public class PropertyDescriptor
    {
        #region 常用属性描述符
        /// <summary>
        /// double值
        /// </summary>
        public readonly static PropertyDescriptor Double = new PropertyDescriptor() {name="SingleDouble", typeName ="double", defaultValue ="0"};
        #endregion

        #region 属性
        /// <summary>
        /// 序号 
        /// </summary>
        internal int xh = 0;
        /// <summary>
        /// 序号 
        /// </summary>
        public int XH { get { return xh; } set { xh = value; } }

        /// <summary>
        /// 名称
        /// </summary>
        internal String name = "";
        /// <summary>
        /// 名称
        /// </summary>
        public String Name { get { return name; } set { name = value; } }

        /// <summary>
        /// 名称
        /// </summary>
        internal String caption = "";
        /// <summary>
        /// 名称
        /// </summary>
        public String Caption { get { return caption; } set { caption = value; } }

        /// <summary>
        /// 别名
        /// </summary>
        internal readonly List<String> alias = new List<string>();
        /// <summary>
        /// 别名
        /// </summary>
        public List<String> Alias { get { return alias; } }
        /// <summary>
        /// 别名集合
        /// </summary>
        public String[] AliaNames { get { return alias.ToArray(); } set { alias.Clear();alias.AddRange(value); } }
        /// <summary>
        /// 类型名
        /// </summary>
        internal String typeName = "String";
        /// <summary>
        /// 类型名
        /// </summary>
        public String TypeName { get { return typeName; } set { typeName = value; } }
        /// <summary>
        /// 类型
        /// </summary>
        public Type Type {
            get {
                return insp.Utility.Reflection.ReflectionUtils.FindType(typeName);
            }
        }


        /// <summary>
        /// 转换格式
        /// </summary>
        internal String format = "";
        /// <summary>
        /// 转换格式
        /// </summary>
        public String Format { get { return format; } set { format = value; } }

        /// <summary>
        /// 缺省值
        /// </summary>
        internal String defaultValue = "";
        /// <summary>
        /// 缺省值
        /// </summary>
        public String DefaultValueText { get { return defaultValue; }set { defaultValue = value; } }
        /// <summary>
        /// 缺省值
        /// </summary>
        public Object DefaultValue { get; }

        /// <summary>
        /// 主属性
        /// </summary>
        internal bool primary = false;
        /// <summary>
        /// 主属性 
        /// </summary>
        public bool Primary { get { return primary; } set { primary = value; } }

        public override string ToString()
        {
            return name + "(Type=" + Type + ")";
        }

        /// <summary>
        /// 必须
        /// </summary>
        internal bool required = false;
        /// <summary>
        /// 必须
        /// </summary>
        public bool Required { get { return required; } set { required = value; } }

        /// <summary>
        /// 构造方法
        /// </summary>
        public PropertyDescriptor() { }

        /// <summary>
        /// 构造方法 
        /// </summary>
        /// <param name="xh"></param>
        /// <param name="name"></param>
        /// <param name="caption"></param>
        /// <param name="alias"></param>
        /// <param name="typeName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="format"></param>
        /// <param name="primary"></param>
        public PropertyDescriptor(int xh,String name,String caption,String[] alias, String typeName,String defaultValue="",String format="",bool required = false,bool primary=false)
        {
            this.xh = xh;
            this.name = name;
            this.caption = caption;
            if(alias != null)
                this.alias.AddRange(alias);
            this.typeName = typeName;
            this.defaultValue = defaultValue;
            this.format = format;
            this.primary = primary;
        }
        #endregion



        #region 名称匹配
        /// <summary>
        /// 名称是否匹配
        /// </summary>
        /// <param name="name">待匹配名称</param>
        /// <param name="ignoreCase">大小写敏感</param>
        /// <param name="includename">名称匹配</param>
        /// <param name="includeCaption">名称匹配</param>
        /// <param name="includeAlias">别名匹配</param>
        /// <returns></returns>
        public bool hasName(String name,bool ignoreCase=true,bool includename=true,bool includeCaption=true,bool includeAlias=true)
        {
            if (name == null || name == "") return false;
            StringComparison sc = ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
            if (includename)
                if (name.Equals(this.name, sc))
                    return true;
            if (includeCaption)
                if (name.Equals(this.caption, sc))
                    return true;
            if (!includeAlias)
                return false;
            foreach(String alia in this.alias)
            {
                if (name.Equals(alia, sc))
                    return true;
            }
            return false;            
        }
        #endregion
    }
}
