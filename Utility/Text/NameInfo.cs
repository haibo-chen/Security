using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Bean;

namespace insp.Utility.Text
{
    /// <summary>
    /// 名称信息
    /// </summary>
    public class NameInfo : INamed
    {
        #region 属性
        private String name = "";
        private String caption = "";
        private String description = "";
        private readonly List<String> alias = new List<string>();
        /// <summary>
        /// 名称
        /// </summary>
        public String Name { get { return name; } set { name = value; } }
        /// <summary>
        /// 名称
        /// </summary>
        public String Caption { get { return caption; } set { caption = value; } }
        /// <summary>
        /// 别名 
        /// </summary>
        public List<String> Alias { get { return alias; } }
        /// <summary>
        /// 描述
        /// </summary>
        public String Description { get { return description; } set { description = value; } }

        #endregion

        #region 初始化
        /// <summary>
        /// 构造方法
        /// </summary>
        public NameInfo()
        {           
        }
        /// <summary>
        /// 构造方法 
        /// </summary>
        /// <param name="name"></param>
        public NameInfo(String name)
        {
            this.name = this.caption = this.description = name;
        }
        public NameInfo(String name, String caption)
        {
            this.name = name;
            this.caption = this.description = caption;
        }
        public NameInfo(String name, String caption, String description)
        {
            this.name = name;
            this.caption = caption;
            this.description = description;
        }

        public NameInfo(String name, String caption, String description, params String[] alias)
        {
            this.name = name;
            this.caption = caption;
            this.description = description;
            this.alias.AddRange(alias);
        }
        public NameInfo(String name, String[] alias)
        {
            this.name = this.caption = this.description = name;
            if (alias != null)
                this.alias.AddRange(alias);
        }

        public override string ToString()
        {
            return ConvertUtils.objectToStr(this);
        }

        public List<String> GetNames()
        {
            List<String> names = new List<string>();
            if (Name != null && Name != "")
                names.Add(Name);
            if (Caption != null && Caption != "")
                names.Add(Caption);
            if (Alias != null && Alias.Count <= 0)
                names.AddRange(Alias);
            return names;
        }

        public bool HasName(String name)
        {
            return this.Name == name || this.Caption == name || (this.Alias != null && this.Alias.Contains(name));
        }
        #endregion
    }
}
