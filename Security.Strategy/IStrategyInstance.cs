using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Bean;

namespace insp.Security.Strategy
{
    /// <summary>
    /// 策略实例状态
    /// </summary>
    public enum StrategyInstanceStatus
    {
        /// <summary>
        /// 尚未初始化
        /// </summary>        
        Creating,
        /// <summary>
        /// 初始化未执行
        /// </summary>
        Idle,
        /// <summary>
        /// 测试运行
        /// </summary>
        Testing,
        /// <summary>
        /// 实际执行
        /// </summary>
        Executing,
        /// <summary>
        /// 执行并正在进行交易
        /// </summary>
        Trading,
    }
    /// <summary>
    /// 策略实例
    /// </summary>
    public interface IStrategyInstance
    {
        #region 基本信息
        /// <summary>
        /// 策略实例ID
        /// </summary>
        String ID { get; }
        /// <summary>
        /// 策略元
        /// </summary>
        IStrategyMeta Meta { get;  }

        /// <summary>
        /// 版本
        /// </summary>
        Version Version { get; }

        #endregion

        #region 初始化和状态
        /// <summary>
        /// 初始化
        /// </summary>
        void Initilization(Properties props=null);
        /// <summary>
        /// 当前状态 
        /// </summary>
        StrategyInstanceStatus Status { get; }
        /// <summary>
        /// 所有参数
        /// </summary>
        Dictionary<PropertyDescriptor, Object> Parameters { get; }
        
        #endregion


        /// <summary>
        /// 执行回测
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        TotalStat DoTest(StrategyContext context,Properties props);

        /// <summary>
        /// 实际执行
        /// </summary>
        void Run(Properties context);

        /// <summary>
        /// 事件触发动作
        /// </summary>
        /// <param name="context"></param>
        /// <param name="args"></param>
        void DoAction(IStrategyContext context, EventArgs args);


    }

    public static class StrategyInstanceUtils
    {
        /// <summary>
        /// 查询指定参数
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static K GetParameterValue<K>(this IStrategyInstance instance,String name)
        {
            KeyValuePair<PropertyDescriptor, Object> kp = instance.Parameters.FirstOrDefault(x => x.Key.hasName(name));
            PropertyDescriptor pd = kp.Key;
            Object value = kp.Value;
            if (value == null) return default(K);
            if (value.GetType() == typeof(K))
                return (K)value;
            return ConvertUtils.ConvertTo<K>(value,pd.Format);
        }
        /// <summary>
        /// 查询指定参数
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Object GetParameterValue(this IStrategyInstance instance,String name)
        {
            KeyValuePair<PropertyDescriptor,Object> kp = instance.Parameters.FirstOrDefault(x => x.Key.hasName(name));
            return kp.Value;
        }
        

    }

    
}
