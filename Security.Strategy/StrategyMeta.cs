using insp.Utility.Bean;
using insp.Utility.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace insp.Security.Strategy
{
    public class StrategyMeta : IStrategyMeta
    {
        #region 基本信息
        /// <summary>
        /// 名称
        /// </summary>
        protected String name;
        /// <summary>
        /// 名称
        /// </summary>
        public String Name { get { return name; } set { name = value; } }
        /// <summary>
        /// 名称
        /// </summary>
        protected String caption;
        /// <summary>
        /// 名称
        /// </summary>
        public String Caption { get { return caption; } set { caption = value; } }
        /// <summary>
        /// 版本
        /// </summary>
        protected Version version;
        /// <summary>
        /// 版本
        /// </summary>
        public Version Version { get { return version; } set { version = value; } }
        /// <summary>
        /// 版本
        /// </summary>
        public String VersionStr { get { return version==null?"":version.ToString(); } set { version = Version.Parse(value); } }

        /// <summary>
        /// 编译文件名
        /// </summary>
        protected String assemblyName;
        /// <summary>
        /// 编译名
        /// </summary>
        public String AssemblyName { get { return assemblyName; } }
        /// <summary>
        /// 实例类名
        /// </summary>
        protected String instanceClassName;
        /// <summary>
        /// 实例类名
        /// </summary>
        public String InstanceClassName { get { return instanceClassName; } }
        #endregion

        #region 参数信息
        protected PropertyDescriptorCollection parameters = new PropertyDescriptorCollection();
        /// <summary>
        /// 策略参数
        /// </summary>
        public PropertyDescriptorCollection Parameters { get { return parameters; } set { parameters = value; } }
        /// <summary>
        /// 策略参数
        /// </summary>
        public List<PropertyDescriptor> PDList { get { return parameters.ToList(); }set { parameters.Clear();parameters.AddRange(value); } }
        #endregion

        #region 策略方法
        /// <summary>
        /// 策略执行方式
        /// </summary>
        protected StrategyExecuteMode mode;
        /// <summary>
        /// 策略执行方式
        /// </summary>
        public StrategyExecuteMode Mode { get { return mode; } }
        /// <summary>
        /// 创建策略实例
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public IStrategyInstance CreateInstance(String id, Properties props,String version)
        {
            Assembly assembly = null;
            if (assemblyName == null && assemblyName != "")
                assembly = TypeUtils.FindAssembly(assemblyName);
            if (assembly == null)
                assembly = typeof(StrategyInstance).Assembly;

            if (instanceClassName == null || instanceClassName == "")
                instanceClassName = "insp.Security.Strategy.StrategyInstance";

            StrategyInstance instance = (StrategyInstance)assembly.CreateInstance(instanceClassName,false,BindingFlags.CreateInstance,null,new object[] {id,props },System.Globalization.CultureInfo.CurrentCulture,null);
            instance.Meta = this;
            return instance;
        }

        /// <summary>
        /// 取得批回测结果标题
        /// </summary>
        /// <returns></returns>
        public String GetBatchResultTitle()
        {
            return "回测编号," + this.GetParameterCaptionString() +
                   ",股票数,回合数,胜率,收益率,总资产,持仓天数(平均/最长),回撤率,每天交易次数(平均/最大)";
        }
        #endregion
    }
}
