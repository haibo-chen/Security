using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Utility.Bean
{
    /// <summary>
    /// 属性描述符集
    /// </summary>
    public class PropertyDescriptorCollection : List<PropertyDescriptor>
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        public PropertyDescriptorCollection() { }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="props"></param>
        public PropertyDescriptorCollection(params PropertyDescriptor[] props)
        {
            if (props == null || props.Length <= 0)
                return;
            this.AddRange(props);
        }
        /// <summary>
        /// 根据名称查找
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        public PropertyDescriptor Get(String propName)
        {
            return this.FirstOrDefault<PropertyDescriptor>(x => x.hasName(propName));
        }
        /// <summary>
        /// 缺省属性
        /// </summary>
        /// <returns></returns>
        public PropertyDescriptor GetDefault()
        {
            if (this.Count == 1)
                return this[0];
            return this.FirstOrDefault<PropertyDescriptor>(x => x.primary);
        }
        /// <summary>
        /// 根据名称索引
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        public int IndexOf(String propName)
        {
            PropertyDescriptor pd = Get(propName);
            return pd == null ? -1 : this.IndexOf(pd);
        }

        
    }
}
