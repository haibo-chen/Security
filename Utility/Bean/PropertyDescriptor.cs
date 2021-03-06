﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

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
        [XmlIgnore]
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
        [XmlAttribute]
        public int XH { get { return xh; } set { xh = value; } }

        /// <summary>
        /// 名称
        /// </summary>
        internal String name = "";
        /// <summary>
        /// 名称
        /// </summary>
        [XmlAttribute]
        public String Name { get { return name; } set { name = value; } }

        /// <summary>
        /// 名称
        /// </summary>
        internal String caption = "";
        /// <summary>
        /// 名称
        /// </summary>
        [XmlAttribute]
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
        [XmlAttribute]
        public String[] AliaNames { get { return alias.ToArray(); } set { alias.Clear();if(value!=null&& value.Length>0)alias.AddRange(value); } }
        /// <summary>
        /// 类型名
        /// </summary>
        internal String typeName = "String";
        /// <summary>
        /// 类型名
        /// </summary>
        [XmlAttribute]
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
        [XmlAttribute]
        public String Format { get { return format; } set { format = value; } }

        /// <summary>
        /// 缺省值
        /// </summary>
        internal String defaultValue = "";
        /// <summary>
        /// 缺省值
        /// </summary>
        [XmlAttribute]
        public String Default { get { return defaultValue; } set { defaultValue = value; } }
        /// <summary>
        /// 缺省值
        /// </summary>
        [XmlIgnore]
        public String DefaultValueText { get { return defaultValue; }set { defaultValue = value; } }
        /// <summary>
        /// 缺省值
        /// </summary>
        [XmlIgnore]
        public Object DefaultValue { get { return ConvertUtils.strToObject(defaultValue, Type, format); } }

        /// <summary>
        /// 主属性
        /// </summary>
        internal bool primary = false;
        /// <summary>
        /// 主属性 
        /// </summary>
        [XmlAttribute]
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
        [XmlAttribute]
        public bool Required { get { return required; } set { required = value; } }

        /// <summary>
        /// 取值范围
        /// </summary>
        protected String rangeText = "";
        /// <summary>
        /// 取值范围
        /// </summary>
        [XmlAttribute]
        public String Range { get { return rangeText; } set { rangeText = value; } }

        

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

        public bool GetRange<T>(out T min, out T max)
        {
            min = default(T);
            max = default(T);

            if (rangeText == null || rangeText == "")
                return false;
            if (!rangeText.Contains("-"))
                return false;
            String[] ss = rangeText.Split('-');
            if (ss == null || ss.Length <= 0)
                return false;

            if(ss[0] != null && ss[0].Trim() != "")
            {
                min = ConvertUtils.ConvertTo<T>(ss[0].Trim());
            }
            if(ss[1] != null && ss[1].Trim() != "")
            {
                max = ConvertUtils.ConvertTo<T>(ss[1].Trim());
            }
            return true;
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
