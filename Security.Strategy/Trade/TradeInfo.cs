using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Text;

namespace insp.Security.Strategy
{
    /// <summary>
    /// 交易信息
    /// </summary>
    public class TradeInfo : IText
    {
        #region 基本交易信息
        /// <summary>
        /// 委托时间
        /// </summary>
        private DateTime entrustDate;
        /// <summary>
        /// 委托时间
        /// </summary>
        [Text(Caption = "委托时间", Format = "yyyyMMddhhmmsss")]
        public DateTime EntrustDate { get { return entrustDate; } set { entrustDate = value; } }


        /// <summary>
        /// 代码
        /// </summary>
        protected String code;
        /// <summary>
        /// 代码
        /// </summary>
        [Text(Caption ="代码")]
        public String Code { get { return code; } set { code = value; } }

        /// <summary>
        /// 交易类型
        /// </summary>
        [Text(Caption = "类型")]
        public TradeDirection Direction { get; set; }

        
        /// <summary>
        /// 委托价格
        /// </summary>
        private double entrustPrice;
        /// <summary>
        /// 委托价格
        /// </summary>
        [Text(Caption = "委托价格", Format = "F2")]
        public double EntrustPrice { get { return entrustPrice; } set { entrustPrice = value; } }


        /// <summary>
        /// 发起本次交易的原因记录
        /// </summary>
        protected String reason;
        /// <summary>
        /// 发起本次交易的原因
        /// </summary>
        [Text(Caption = "交易原因")]
        public String Reason { get { return reason; } set { reason = value; } }

        /// <summary>
        /// 交易时间
        /// </summary>
        private DateTime tradeDate = DateTime.Now;
        /// <summary>
        /// 交易时间
        /// </summary>
        [Text(Caption = "交易时间", Format = "yyyyMMddhhmmsss")]
        public DateTime TradeDate { get { return tradeDate; } set { tradeDate = value; } }
        /// <summary>
        /// 交易数量(股数)
        /// </summary>
        public int Amount { get; set; }


        /// <summary>
        /// 交易价格
        /// </summary>
        [Text(Caption = "交易价格", Format = "F2")]
        public double TradePrice { get; set; }

        public const String TM_AUTO = "自动交易";
        public const String TM_MAUAL = "手工交易";
        /// <summary>
        /// 交易方式
        /// </summary>
        private String tradeMethod = TM_AUTO;
        /// <summary>
        /// 交易方式
        /// </summary>
        [Text(Caption = "交易方式")]
        public String TradeMethod { get { return tradeMethod; } set { tradeMethod = value; } }
        /// <summary>
        /// 手续费
        /// </summary>
        [Text(Caption = "手续费")]
        public double Fee { get; set; }
        /// <summary>
        /// 印花税
        /// </summary>
        [Text(Caption = "印花税")]
        public double Stamps { get; set; }
        #endregion

        #region 统计性信息
        /// <summary>
        /// 交易金额(价格*数量)
        /// </summary>
        [Text(Caption = "交易金额(价格*数量)",Format ="F2")]
        public double Turnover
        {
            get { return TradePrice * Amount; }
        }
        /// <summary>
        /// 交易费用（手续费和印花税上的花销）
        /// </summary>
        [insp.Utility.Common.Transinet]
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
        [Text(Caption = "交易花销", Format = "F2")]
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
            return code + "," + 
                (Direction == TradeDirection.Buy ? "买入" : "卖出") +
                ",委托日期=" + this.entrustDate.ToString("yyyyMMdd") +
                ",委托价格=" + this.entrustPrice.ToString("F2") +
                ",数量=" + this.Amount.ToString() + 
                ",交易日期=" + this.TradeDate.ToString("yyyyMMdd") + 
                ",交易价格=" + TradePrice.ToString("F2") + ",总金额=" + Turnover.ToString("F2")+(reason==null||reason==""?"":","+reason);
        }

    }
}
