using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Strategy
{
    /// <summary>
    /// 交易意向类型
    /// </summary>
    public enum TradeIntent
    {
        /// <summary>
        /// 不确定 
        /// </summary>
        Unknown = 0,
        /// <summary>
        ///空头
        /// </summary>
        Short=1,
        /// <summary>
        /// 多头
        /// </summary>
        bull =2
    }
}
