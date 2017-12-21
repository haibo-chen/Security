using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Bean;

namespace insp.Security.Strategy
{
    /// <summary>
    /// 策略执行方式
    /// </summary>
    public enum StrategyExecuteMode
    {
        /// <summary>
        /// 下面两种都可以
        /// </summary>
        Both,
        /// <summary>
        /// 事件驱动
        /// </summary>
        Event,
        /// <summary>
        /// 定时执行
        /// </summary>
        Timer,
    }

    /// <summary>
    /// 策略元信息
    /// </summary>
    public interface IStrategyMeta
    {
        #region 基本信息
        /// <summary>
        /// 名称
        /// </summary>
        String Name { get; }
        /// <summary>
        /// 名称
        /// </summary>
        String Caption { get; }
        /// <summary>
        /// 版本
        /// </summary>
        Version Version { get; }
        #endregion

        #region 参数信息
        /// <summary>
        /// 策略参数
        /// </summary>
        PropertyDescriptorCollection Parameters { get; }
        #endregion

        #region 策略方法
        /// <summary>
        /// 策略执行方式
        /// </summary>
        StrategyExecuteMode Mode { get; }
        /// <summary>
        /// 创建策略实例
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        IStrategyInstance CreateInstance(String id, Properties props,String version);

        /// <summary>
        /// 取得批回测结果标题
        /// </summary>
        /// <returns></returns>
        String GetBatchResultTitle();
        #endregion

    }

    public static class StrategyMetaUtils
    {
        public static List<String> GetParameterCaptions(this IStrategyMeta meta)
        {
            List<String> results = new List<string>();
            meta.Parameters.ForEach(x => results.Add(x.Caption));
            return results;
        }

        public static List<String> GetParameterNames(this IStrategyMeta meta)
        {
            List<String> results = new List<string>();
            meta.Parameters.ForEach(x => results.Add(x.Name));
            return results;
        }
        public static String GetParameterNameString(this IStrategyMeta meta)
        {
            return meta.Parameters.ConvertAll(x => x.Name).Aggregate((x, y) => x + "," + y);
        }

        public static String GetParameterCaptionString(this IStrategyMeta meta)
        {
            return meta.Parameters.ConvertAll(x => x.Caption).Aggregate((x, y) => x + "," + y);
        }

        public static bool HasName(this IStrategyMeta meta,String name)
        {
            return meta.Caption == name || meta.Name == name;

        }
    }
}
