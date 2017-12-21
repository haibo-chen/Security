using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Strategy
{
    /// <summary>
    /// 交易信息
    /// </summary>
    public class TradeInfo
    {
        #region 基本交易信息
        /// <summary>
        /// 代码
        /// </summary>
        protected String code;
        /// <summary>
        /// 代码
        /// </summary>
        public String Code { get { return code; } set { code = value; } }

        /// <summary>
        /// 发起本次交易的原因记录
        /// </summary>
        protected String reason;
        /// <summary>
        /// 发起本次交易的原因
        /// </summary>
        public String Reason { get { return reason; } set { reason = value; } }
        /// <summary>
        /// 交易类型
        /// </summary>
        public TradeDirection Direction { get; set; }

        private DateTime tradeDate = DateTime.Now;
        /// <summary>
        /// 交易时间
        /// </summary>
        public DateTime TradeDate { get { return tradeDate; } set { tradeDate = value; } }
        /// <summary>
        /// 交易数量(股数)
        /// </summary>
        public int Amount { get; set; }
        /// <summary>
        /// 交易价格
        /// </summary>
        public double TradePrice { get; set; }
        /// <summary>
        /// 手续费
        /// </summary>
        public double Fee { get; set; }
        /// <summary>
        /// 印花税
        /// </summary>
        public double Stamps { get; set; }
        #endregion

        #region 统计性信息
        /// <summary>
        /// 交易金额(价格*数量)
        /// </summary>
        public double Turnover
        {
            get { return TradePrice * Amount; }
        }
        /// <summary>
        /// 交易费用（手续费和印花税上的花销）
        /// </summary>
        public double TradeFee
        {
            get
            {
                if (Direction == TradeDirection.Buy)
                    return TradePrice * Amount * Fee;
                return TradePrice * Amount * Fee + TradePrice * Amount * Stamps;
            }
        }
        /// <summary>
        /// 交易花销
        /// 买入就是花掉的钱（交易金额(价格*数量)+交易费用）
        /// 卖出就是收回的钱（交易金额(价格*数量)-交易费用）
        /// </summary>
        public double TradeCost
        {
            get { return this.Direction == TradeDirection.Buy?Turnover + TradeFee: Turnover - TradeFee; }
        }

        #endregion

        public String ToText(bool showCode)
        {
            if (!showCode)
                return ToString();
            else
                return code + ":" + ToString();
        }
        public override string ToString()
        {
            return (Direction == TradeDirection.Buy ? "买入" : "卖出") + ",日期=" + this.TradeDate.ToString("yyyyMMdd") + ",价格=" + TradePrice.ToString("F2") + ",总金额=" + Turnover.ToString("F2")+(reason==null||reason==""?"":","+reason);
        }

    }
}
