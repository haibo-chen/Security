using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Utility.Text
{
    public class TextAttribute : Attribute
    {
        public String Name { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public String Caption { get; set; }
        /// <summary>
        /// 别名 
        /// </summary>
        public String[] Alias { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// 忽略
        /// </summary>
        public bool Ignored { get; set; }

        /// <summary>
        /// 格式
        /// </summary>
        public String Format { get; set; }

        /// <summary>
        /// 可以显示的文本
        /// </summary>
        public String ShowText
        {
            get
            {
                if (Caption != null && Caption != "")
                    return Caption;
                return Alias.FirstOrDefault<String>(x => x != null && x != "");                
            }
        }

        public List<String> GetNames()
        {
            List<String> names = new List<string>();
            if (Name != null && Name != "")
                names.Add(Name);
            if (Caption != null && Caption != "")
                names.Add(Caption);
            if (Alias != null && Alias.Length <= 0)
                names.AddRange(Alias);
            return names;
        }

        public bool HasName(String name)
        {
            return this.Name == name || this.Caption == name || (this.Alias != null && this.Alias.Contains(name));
        }
        public int HasName(String[] names)
        {
            if (names == null || names.Length <= 0) return -1;
            for(int i=0;i<names.Length;i++)
            {
                if (HasName(names[i]))
                    return i;
            }
            return -1;
        }
    }
}
