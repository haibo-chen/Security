using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;


namespace insp.Utility.Bean
{
    /// <summary>
    /// 属性集
    /// </summary>
    public class Properties
    {
        #region 元信息
        /// <summary>
        /// 元信息
        /// </summary>
        private static Dictionary<Type,PropertyDescriptorCollection> pdcs = new Dictionary<Type, PropertyDescriptorCollection>();
        /// <summary>
        /// 元验证是否有效
        /// </summary>
        private static Dictionary<Type, bool> metaValidate = new Dictionary<Type, bool>();
        /// <summary>
        /// 注册元信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pdc"></param>
        public static void RegistePropertyDescriptorCollection(Type type, PropertyDescriptorCollection pdc)
        {
            if (pdcs.ContainsKey(type))
                pdcs[type] = pdc;
            else
                pdcs.Add(type,pdc);
        }
        /// <summary>
        /// 取得包含key为名称的属性描述符
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public PropertyDescriptor GetPropertyDescriptor(String key)
        {
            if (!pdcs.ContainsKey(this.GetType()))
                return null;
            return pdcs[this.GetType()].Get(key);
        }
        /// <summary>
        /// 修改元验证有效信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public static void SetMetaValidate(Type type,bool value)
        {
            if (metaValidate.ContainsKey(type))
                metaValidate[type] = value;
            else
                metaValidate.Add(type, value);
        }
        /// <summary>
        /// 是否使用元信息进行验证
        /// </summary>
        public bool MetaValidate {
            get
            {
                Type type = this.GetType();
                if (!pdcs.ContainsKey(type))
                    return false;
                PropertyDescriptorCollection t = pdcs[type];
                if (t == null || t.Count <= 0) return false;

                if (!metaValidate.ContainsKey(type))
                    return true;
                return metaValidate[type];
            }
            set
            {
                Type type = this.GetType();
                if (metaValidate.ContainsKey(type))
                    metaValidate[type] = value;
                else
                    metaValidate.Add(type,value);
            }
           
        }
        #endregion

        #region 基本属性
        /// <summary>
        /// KEY集合
        /// </summary>
        protected List<String> keys = new List<string>();
        /// <summary>
        /// 值集合
        /// </summary>
        protected Dictionary<String, Object> values = new Dictionary<string, Object>();
        /// <summary>
        /// 每个KEY，value对应的注释信息
        /// </summary>
        protected Dictionary<String, String> followComments = new Dictionary<string, string>();
        /// <summary>
        /// 每个KEY，value对应的注释信息
        /// </summary>
        protected Dictionary<String, List<String>> beforeComments = new Dictionary<string, List<String>>();
        /// <summary>
        /// KEY集合
        /// </summary>
        public List<String> Keys { get { return new List<string>(keys); } }
        
        #endregion
        #region 文件读写
        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns></returns>
        public Properties Clone()
        {
            Properties prop = new Properties();
            prop.keys.AddRange(keys);
            prop.values = new Dictionary<string, object>(values);
            prop.followComments = new Dictionary<string, string>(followComments);
            prop.beforeComments = new Dictionary<string, List<string>>(beforeComments);
            return prop;
        }
        /// <summary>
        /// 克隆
        /// </summary>
        /// <param name="prop"></param>
        public void Clone(Properties prop)
        {
            if (prop == null)
                return;
            this.keys.AddRange(prop.keys);
            this.values = new Dictionary<string, object>(prop.values);
            this.followComments = new Dictionary<string, string>(prop.followComments);
            this.beforeComments = new Dictionary<string, List<string>>(prop.beforeComments);
        }
        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static Properties Load(String filename,Encoding encode)
        {
            if (!File.Exists(filename))
                return new Properties();

            Properties prop = new Properties();
            List<String> comments = new List<string>();

            String[] lines = File.ReadAllLines(filename, encode);
            for(int i=0;i<lines.Length;i++)
            {
                String line = lines[i];
                if (line == null || line == "" || line.Trim() == "")
                    continue;
                line = line.Trim();
                if(line.StartsWith("#"))
                {
                    comments.Add(line);
                    continue;
                }
                String[] ss = line.Split('=');
                if (ss == null || ss.Length <= 0||ss[0]==null) continue;
                String key = ss[0].Trim();
                String value = ss.Length <= 1 || ss[1] == null ? "" : ss[1].Trim();
                int index = value.IndexOf("#");
                if (index >= 0)
                {
                    value = value.Substring(0, index);
                    prop.followComments.Add(key,value.Substring(index,value.Length-index));
                }
                    
                if (comments.Count > 0)
                {
                    prop.beforeComments.Add(key, comments);
                    comments = new List<string>();
                }
                prop.keys.Add(key);
                prop.values.Add(key,value);
                
            }
            return prop;
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="encode"></param>
        public void Save(String filename,Encoding encode)
        {
            List<String> lines = new List<string>();
            foreach(String key in keys)
            {
                if(beforeComments.ContainsKey(key))
                {
                    lines.AddRange(beforeComments[key]);
                }
                String comments = "";
                if(followComments.ContainsKey(key))
                {
                    comments = "               "+followComments[key];
                }
                lines.Add(key + "=" + values[key] + comments);
            }
            File.WriteAllLines(filename, lines.ToArray(), encode);
        }
        #endregion

        #region 添加删除属性

        public bool ContainKey(String key)
        {
            if(!MetaValidate)
                return this.values.ContainsKey(key);
            PropertyDescriptor pd = this.GetPropertyDescriptor(key);
            if(pd == null)
                return this.values.ContainsKey(key);
            return this.values.ContainsKey(pd.name);
        }

        public void Put(String key,Object value)
        {
            if (MetaValidate)
            {
                PropertyDescriptor pd = this.GetPropertyDescriptor(key);
                if (pd != null)
                    key = pd.name;
            }
            if (!keys.Contains(key))
                keys.Add(key);
            if (values.ContainsKey(key))
                values[key] = value;
            else values.Add(key,value);            
        }
        public T Get<T>(String key)
        {
            return Get<T>(key, default(T), true);
        }
        public T Get<T>(String key, T defaultValue)
        {
            return Get<T>(key, defaultValue, false);
        }
        public T Get<T>(String key, T defaultValue=default(T),bool throwException=false)
        {
            PropertyDescriptor pd = null;
            if (MetaValidate)
            {
                pd = this.GetPropertyDescriptor(key);
                if (pd != null)
                    key = pd.name;
            }

            if (!values.ContainsKey(key))
            {
                if (MetaValidate && pd != null)
                    return defaultValue;
                if (throwException)
                    throw new Exception(key+"不存在");
                return defaultValue;
            }
            Object value = values[key];
            if (value == null) return defaultValue;
            if (value.GetType()==typeof(T) || value.GetType().IsSubclassOf(typeof(T)))
                return (T)value;
            return ConvertUtils.ConvertTo<T>(value);
        }

        public Object this[String key]
        {
            get { return Get<Object>(key); }
            set { Put(key, value); }
        }
        #endregion

        #region 特殊处理
        /// <summary>
        /// 分裂处理
        /// /// 例如配置中是:
        /// key1.x.a = 1
        /// key1.x.b = 2
        /// key2.y.c = 3
        /// key2.y.d = 4
        /// 将返回
        /// key1 => {x.a=1,x.b=2}, key2=>{y.c=3,y.d=4}
        /// </summary>
        /// <returns></returns>
        public Dictionary<String, Properties> Spilt()
        {
            Dictionary<String, Properties> results = new Dictionary<string, Properties>();
            List<String> keyPrefixs = new List<string>();

            for (int i = 0; i < keys.Count; i++)
            {
                String keyPrefix = keys[i];
                String newkey = keys[i];
                if (keyPrefix.Contains("."))
                {
                    int index = keyPrefix.IndexOf(".");
                    newkey = keyPrefix.Substring(index + 1, keyPrefix.Length - index - 1);
                    keyPrefix = keyPrefix.Substring(0, index);                    
                }
                    
                
                if (!results.ContainsKey(keyPrefix))
                    results.Add(keyPrefix, new Properties());
                Properties childProps = results[keyPrefix];
                childProps.keys.Add(newkey);
                childProps.values.Add(newkey,this[keys[i]]);
                if(this.followComments.ContainsKey(keys[i]))
                    childProps.followComments.Add(newkey, this.followComments[keys[i]]);
                if(this.beforeComments.ContainsKey(keys[i]))
                    childProps.beforeComments.Add(newkey, this.beforeComments[keys[i]]);
            }

            return results;
        }
        /// <summary>
        /// 分裂处理
        /// 例如配置中是:
        /// key.x.a = 1
        /// key.x.b = 2
        /// key.y.c = 3
        /// key.y.d = 4
        /// 将返回
        /// x => {a=1,b=2}, y=>{c=3,b=4}
        /// </summary>
        /// <returns></returns>
        public Dictionary<String,Properties> Spilt(String key)
        {
            Dictionary<String, Properties> results = new Dictionary<string, Properties>();

            for (int i=0;i<keys.Count; i++)
            {
                String k = keys[i];

                if (!k.StartsWith(key)) continue;
                int n1 = k.IndexOf(key) + key.Length + 1;
                int n2 = k.LastIndexOf(".");
                String nk = k.Substring(n1,n2-n1);
                Properties ps = null;
                if (!results.ContainsKey(nk))
                    results.Add(nk,new Properties());
                ps = results[nk];

                String tk = k.Substring(n2 + 1);
                ps.keys.Add(tk);
                ps.values.Add(tk,values[k]);
                if (followComments.ContainsKey(k))
                    ps.followComments.Add(tk, followComments[k]);
                if (beforeComments.ContainsKey(k))
                    ps.beforeComments.Add(tk, beforeComments[k]);
            }
            return results;
        }
        #endregion
    }
}
