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

        public String ToString(Properties props)
        {
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < this.Count; i++)
            {
                PropertyDescriptor pd = this[i];
                if (str.ToString() != "")
                    str.Append(",");
                str.Append(pd.caption+"="+(props==null?pd.Default:props.Get<String>(pd.name)));

            }
            return str.ToString();
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

        
        public List<Property> Check(List<Property> props)
        {
            List<Property> result = new List<Property>();
            for(int i=0;i<this.Count;i++)
            {
                PropertyDescriptor pd = this[i];
                Property prop = props.FirstOrDefault(x => x.Name == pd.name);
                if (prop == null) result.Add(new Property() { Name = pd.name, Value = pd.DefaultValueText });
                else
                {
                    result.Add(prop);
                    props.Remove(prop);
                }
            }
            if (props.Count > 0)
                result.AddRange(props);
            return result;
        }

        public Properties CreateProperties(Properties props)
        {

            Properties r = new Properties();
            if (props != null)
                r = props.Clone();

            for (int i = 0; i < this.Count; i++)
            {
                PropertyDescriptor pd = this[i];
                String value = pd.DefaultValueText;
                if (r.ContainKey(pd.Name))
                    continue;
                r.Put(pd.Name, pd.DefaultValueText);
            }
            return r;
        }
        
    }
}
