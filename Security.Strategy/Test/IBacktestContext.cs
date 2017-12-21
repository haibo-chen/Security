using insp.Utility.Bean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Strategy
{
    /// <summary>
    /// 回测上下文
    /// </summary>
    public interface IBacktestContext
    {
        /// <summary>
        /// 初始资金
        /// </summary>
        double InitFund { get; }
        /// <summary>
        /// 测试开始日期
        /// </summary>
        DateTime BeginDate { get; }
        /// <summary>
        /// 测试结束日期
        /// </summary>
        DateTime EndDate { get; }
        /// <summary>
        /// 批号
        /// </summary>
        int BatchNo { get; }
        /// <summary>
        /// 结果路径
        /// </summary>
        String Resultpath { get; }
        /// <summary>
        /// 代码文件
        /// </summary>
        String CodeFile { get; }
        /// <summary>
        /// 并行
        /// </summary>
        bool Parallel { get; }
        /// <summary>
        /// 交易手续费
        /// </summary>
        double Volumecommission { get; }
        /// <summary>
        /// 印花税
        /// </summary>
        double Stampduty { get; }
        /// <summary>
        /// 参数信息
        /// </summary>
        Properties FileParamters { get; }



    }
}
