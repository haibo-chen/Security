using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace insp.Security.Trade
{
    /// <summary>
    /// 委托事件回调
    /// </summary>
    /// <param name="tradeAgent"></param>
    /// <param name="request"></param>
    /// <param name="param"></param>
    /// <param name="result"></param>
    public delegate void HandleEntrust(ITradeBroker tradeAgent, EntrustRequest request, EntrustParam param, TradeResult result);

    /// <summary>
    /// 交易代理
    /// </summary>
    public interface ITradeBroker
    {
        #region 基本信息
        /// <summary>
        /// 名称
        /// </summary>
        String Name { get; }
        /// <summary>
        /// 提供商
        /// </summary>
        String Vendor { get; }

        
        #endregion

        #region 单个下单
        /// <summary>
        /// 单个下单(同步)
        /// </summary>
        /// <param name="code">代码</param>
        /// <param name="price">价格</param>
        /// <param name="amount">数量</param>
        /// <param name="retryNum">重试次数</param>
        /// <param name="maxQueryNum">最大查询次数</param>
        /// <param name="queryInterval">查询时间</param>
        /// <param name="entrustType">委托类型</param>
        /// <returns></returns>
        TradeResult DoEntrust(String code, double price, double amount, int retryNum = 0, int maxQueryNum = 0, int queryInterval = 0, EntrustType entrustType = EntrustType.LimitOrder);
        /// <summary>
        /// 单个下单(同步)
        /// </summary>
        /// <param name="request">委托请求对象</param>
        /// <param name="param">委托操作参数对象</param>
        /// <returns></returns>
        TradeResult DoEntrust(EntrustRequest request, EntrustParam param);

        /// <summary>
        /// 单个下单(异步)
        /// </summary>
        /// <param name="code">代码</param>
        /// <param name="price">价格</param>
        /// <param name="amount">数量</param>
        /// <param name="retryNum">重试次数</param>
        /// <param name="maxQueryNum">最大查询次数</param>
        /// <param name="queryInterval">查询时间</param>
        /// <param name="entrustType">委托类型</param>
        /// <returns></returns>
        EntrustrResult BeginEntrust(String code, double price, double amount, HandleEntrust handler,int retryNum = 0, int maxQueryNum = 0, int queryInterval = 0, EntrustType entrustType = EntrustType.LimitOrder);
        /// <summary>
        /// 单个下单(异步)
        /// </summary>
        /// <param name="request">委托请求对象</param>
        /// <param name="param">委托操作参数对象</param>
        /// <returns></returns>
        EntrustrResult BeginEntrust(EntrustRequest request, HandleEntrust handler, EntrustParam param=null);
        #endregion

        #region 组合下单
        #endregion

        #region 撤单
        #endregion

        #region 资金查询
        #endregion

        #region 委托查询
        #endregion

        #region 成交查询
        #endregion
    }
}
