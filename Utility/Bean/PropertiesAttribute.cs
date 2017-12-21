using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Utility.Bean
{
    /// <summary>
    /// 属性
    /// </summary>
    public class PropertiesAttribute : Attribute
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        public PropertiesAttribute() { }
        /// <summary>
        /// 构造方法 
        /// </summary>
        /// <param name="items"></param>
        public PropertiesAttribute(params PropertyDescriptor[] items)
        {
            this.items = items;
        }
        /// <summary>
        /// 属性集合
        /// </summary>
        internal PropertyDescriptor[] items;
        /// <summary>
        /// 属性集合
        /// </summary>
        public PropertyDescriptor[] Items { get { return items; } set { items = value; } }
    }
}
