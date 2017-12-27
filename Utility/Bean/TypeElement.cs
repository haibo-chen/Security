using insp.Utility.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using insp.Utility.Collections;

namespace insp.Utility.Bean
{
    /// <summary>
    /// 属性元素 
    /// </summary>
    public class Property
    {
        /// <summary>
        /// 名称 
        /// </summary>
        [XmlAttribute]
        public String Name;
        /// <summary>
        /// 属性
        /// </summary>
        [XmlAttribute]
        public String Value;

        public override string ToString()
        {
            return "Name=" + Name + ",Value=" + Value;
        }
    }
    /// <summary>
    /// 属性元素
    /// </summary>
    public class PropertiesElement
    {
        [XmlArray]
        [XmlArrayItem(Type = typeof(Property), ElementName = "property")]
        public List<Property> properties = new List<Property>();
    }

    /// <summary>
    /// 类型元素
    /// </summary>
    public class TypeDescriptorElement
    {
        [XmlAttribute]
        public String Name;
        [XmlAttribute]
        public String Caption;
        [XmlAttribute]
        public String Assembly;
        [XmlAttribute]
        public String ClassName;

        [XmlArray]
        [XmlArrayItem(Type = typeof(PropertyDescriptor), ElementName = "property")]
        public List<PropertyDescriptor> properties = new List<PropertyDescriptor>();

        public override string ToString()
        {
            return Name + "," + Caption + "," + Assembly + "," + ClassName;
        }
        public T CreateType<T>()
        {
            Assembly assembly = TypeUtils.LoadAssemby(Assembly);
            if (assembly == null) return default(T);
            //Type type = assembly.GetTypes().Find<Type>("FullName", ClassName);
            //if (type == null) return default(T);

            T obj = (T)assembly.CreateInstance(ClassName);
            if (obj == null) return default(T);

            obj.SetMemberValue("Name", Name);
            obj.SetMemberValue("Caption", Caption);
            obj.SetMemberValue("PDList", properties);

            return obj;
        }
    }
    

    

    /// <summary>
    /// 类型元素
    /// </summary>
    public class TypePropertyElement
    {
        [XmlAttribute]
        public String Name;
        [XmlAttribute]
        public String Caption;
        [XmlAttribute]
        public String Assembly;
        [XmlAttribute]
        public String ClassName;

        [XmlArray]
        [XmlArrayItem(Type = typeof(Property), ElementName = "property")]
        public List<Property> properties = new List<Property>();

        public override string ToString()
        {
            return Name + "," + Caption + "," + Assembly + "," + ClassName;
        }

        public T CreateType<T>()
        {
            if (ClassName == null || ClassName == "")
                ClassName = typeof(T).FullName;
            KeyValuePair<Assembly,Type> kv = ReflectionUtils.FindAssemblyAndType(ClassName);
            Assembly assembly = kv.Key;
            Type t = kv.Value;
            if (kv.Value == null)
            {
                if (Assembly == null || Assembly == "")
                    return default(T);
                assembly = TypeUtils.LoadAssemby(Assembly);
                t = ReflectionUtils.FindType(ClassName);
            }
            if (t == null)
                return default(T);
            T obj = (T)t.Assembly.CreateInstance(ClassName);
            if (obj == null) return default(T);

            obj.SetMemberValue("Name", Name);
            obj.SetMemberValue("Caption", Caption);

            
            return obj;
        }
    }
}
