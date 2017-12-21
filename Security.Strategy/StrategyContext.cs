using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Bean;

namespace insp.Security.Strategy
{
    /// <summary>
    /// 策略上下文
    /// </summary>
    public class StrategyContext : Properties,IStrategyContext
    {
        #region 单体
        /// <summary>
        /// 实例
        /// </summary>
        private static StrategyContext instance;
        /// <summary>
        /// 缺省实例
        /// </summary>
        public static StrategyContext Default
        {
            get { if (instance == null) instance = new StrategyContext();return instance; }
        }
        /// <summary>
        /// 构造方法 
        /// </summary>
        private StrategyContext() { }
        #endregion

    }
}
