using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

using insp.Utility.Bean;
using insp.Utility.Text;

namespace insp.Security.Data.Security
{
    /// <summary>
    /// 证劵类别
    /// </summary>
    public enum SecurityCataory
    {
        /// <summary>股票</summary>
        Stock = 1,
        /// <summary>期货</summary>
        Futures,
        /// <summary>指数</summary>
        Index,
        /// <summary>基金</summary>
        Funds

    }
    /// <summary>
    /// 证劵信息
    /// </summary>
    public class SecurityInfo : DynamicObject
    {
        /// <summary>
        /// 值
        /// </summary>
        private Dictionary<String, Object> values = new Dictionary<string, object>();
        /// <summary>
        /// 构造方法
        /// </summary>
        public SecurityInfo() { }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="values"></param>
        public SecurityInfo(Dictionary<String, Object> values)
        {
            if (values == null || values.Count <= 0) return;
            this.values = values;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var name = binder.Name;
            return values.TryGetValue(name, out result);
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {            
            var name = binder.Name;
            if (!values.ContainsKey(name))
                return false;
            values[name] = value;
            return true;
        }
        
    }

    /// <summary>
    /// 证劵信息
    /// </summary>
    public class SecurityProperties : Properties
    {
        #region 元信息
        /// <summary>代码</summary>  
        public static readonly PropertyDescriptor P_CODE = new PropertyDescriptor(1, "code", "代码", null, "String");
        /// <summary>名称</summary>
        public static readonly PropertyDescriptor P_NAME = new PropertyDescriptor(2, "name", "名称", null, "String");
        /// <summary>全称</summary>   
        public static readonly PropertyDescriptor P_FULLNAME = new PropertyDescriptor(3, "fullname", "全称", null, "String");
        /// <summary>上市日期</summary>   
        public static readonly PropertyDescriptor P_LAUNCHSDATE = new PropertyDescriptor(4, "launchdate", "上市日期", null, "String");
        /// <summary>市场</summary>   
        public static readonly PropertyDescriptor P_MARKET = new PropertyDescriptor(5, "market", "市场", null, "String");
        /// <summary>类别</summary>   
        public static readonly PropertyDescriptor P_CATAORY = new PropertyDescriptor(6, "cataory", "类别", null, "String");
        /// <summary>行业</summary>   
        public static readonly PropertyDescriptor P_INDUSTRY = new PropertyDescriptor(7, "industry", "行业", null, "String");
        /// <summary>板块</summary>   
        public static readonly PropertyDescriptor P_BLOCKS = new PropertyDescriptor(8, "blocks", "板块", null, "String");
        /// <summary>地区</summary>   
        public static readonly PropertyDescriptor P_REGION = new PropertyDescriptor(9, "range", "地区", null, "String");
        /// <summary>交易所（简称）</summary>   
        public static readonly PropertyDescriptor P_EXCHANGE = new PropertyDescriptor(10, "exchange", "交易所", null, "String");
        
        /// <summary>属性集</summary>   
        public static readonly PropertyDescriptorCollection PDC = new PropertyDescriptorCollection(P_CODE, P_NAME, P_FULLNAME, P_LAUNCHSDATE,P_MARKET, P_CATAORY, P_INDUSTRY, P_BLOCKS, P_REGION, P_EXCHANGE);

        /// <summary>
        /// 静态初始化
        /// </summary>
        static SecurityProperties()
        {            
            RegistePropertyDescriptorCollection(typeof(SecurityProperties),PDC);            
        }
        #endregion


        #region 属性名

        /// <summary>
        /// 证劵代码
        /// </summary>
        public String Code { get { return this.Get<String>(P_CODE.Name); } }
        /// <summary>
        /// 证劵名称
        /// </summary>
        public String Name { get { return this.Get<String>(P_NAME.Name); } }

        /// <summary>
        /// 证劵名称
        /// </summary>
        public String FullName { get { return this.Get<String>(P_FULLNAME.Name); } }

        /// <summary>
        /// 证劵名称
        /// </summary>
        public DateTime LaunchDate { get { return this.Get<DateTime>(P_LAUNCHSDATE.Name); } }


        /// <summary>
        /// 市场名称
        /// </summary>
        public String Market { get { return this.Get<String>(P_MARKET.Name); } }

        /// <summary>
        /// 类别(股票、期货、指数、基金)
        /// </summary>
        public String Cataory { get { return this.Get<String>(P_CATAORY.Name); } }

        /// <summary>
        /// 行业
        /// </summary>
        public String Industry { get { return this.Get<String>(P_INDUSTRY.Name); } }

        /// <summary>
        /// 板块（上证A，深证A,中小板，创业板）
        /// </summary>
        public String Blocks { get { return this.Get<String>(P_BLOCKS.Name); } }

        /// <summary>
        /// 地区
        /// </summary>
        public String Region { get { return this.Get<String>(P_REGION.Name); } }

        /// <summary>
        /// 交易所名称
        /// </summary>
        public String Exchange { get { return this.Get<String>(P_EXCHANGE.Name); } }

        #endregion

        #region 投影
        /// <summary>
        /// 查询证劵信息
        /// </summary>
        /// <param name="propNames"></param>
        /// <returns></returns>
        public SecurityInfo Select(params String[] propNames)
        {
            dynamic info = new SecurityInfo();
            info.Code = this[P_CODE.Name];
            if (propNames == null || propNames.Length <= 0)
            {                
                info.Name = this[P_NAME.Name];
                return info;
            }

            Dictionary<String, Object> values = new Dictionary<string, object>();
            foreach (String propName in propNames)
            {
                if (!P_CODE.hasName(propName.ToLower()))
                    continue;                
                values.Add(propName,this[propName]);
            }
            return new SecurityInfo(values);
        }
        #endregion

        #region 读写
        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="str"></param>
        /// <param name="propNames"></param>
        /// <param name="sep"></param>
        /// <returns></returns>
        public static SecurityProperties Parse(String str,String[] propNames,String sep=",")
        {
            String[] ss = str.Spilt2(sep.ToCharArray());
            if (ss == null || ss.Length < 0)
                return null;
            if (ss.Length > propNames.Length)
                throw new Exception("属性集数量不足");
            SecurityProperties sp = new SecurityProperties();
            for(int i=0;i<ss.Length;i++)
            {
                sp[propNames[i]] = ss[i];
            }
            return sp;
        }
        /// <summary>
        /// 字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(null);
        }
        /// <summary>
        /// 字符串
        /// </summary>
        /// <param name="propNames"></param>
        /// <returns></returns>
        public String ToString(params String[] propNames)
        {
            if (propNames == null || propNames.Length <= 0)
                propNames = PDC.ConvertAll(x => x.Name).ToArray();
            StringBuilder str = new StringBuilder();
            return propNames.ToList().ConvertAll(x => this.Get<String>(x)).Aggregate((x, y) => x + "," + y);

        }


        #endregion
    }
}
