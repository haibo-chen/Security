using insp.Utility.Bean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Strategy
{
    /// <summary>
    /// 交易回合
    /// </summary>
    public class TradeBout : IComparable<TradeBout>
    {
        #region 基本信息
        /// <summary>
        /// 代码
        /// </summary>
        protected String code;
        /// <summary>
        /// 代码
        /// </summary>
        public String Code { get { return code; } set { code = value; } }
        /// <summary>
        /// 交易信息
        /// </summary>
        protected TradeInfo[] tradeInfos = new TradeInfo[2];

        public TradeBout(String code) { this.code = code; }
        #endregion

        #region 字符串转换
        public String ToText()
        {
            return code + "," + ToString();
            //return code + "#" + (tradeInfos[0]==null?"":ConvertUtils.objectToStr(tradeInfos[0])) + " > " + (tradeInfos[1]==null?"":ConvertUtils.objectToStr(tradeInfos[1]));
        }
        public static TradeBout Parse(String str)
        {
            if (str == null) return null;

            TradeBout bout = null;
            String[] ss = str.Split('#');
            if (ss == null || ss.Length < 2)
                return new TradeBout(ss[0] == null ? "" : ss[0].Trim());
            bout = new TradeBout(ss[0] == null ? "" : ss[0].Trim());

            if (ss[1] == null || ss[1] == "") return bout;

            String[] sss = ss[1].Split('>');
            if (sss == null || sss.Length <= 0)
                return bout;
            if (sss[0] != null && sss[0].Trim() != "")
                bout.tradeInfos[0] = ConvertUtils.strToObject<TradeInfo>(sss[0].Trim());
            if (sss.Length >= 2 && sss[1] != null && sss[1].Trim() != "")
                bout.tradeInfos[1] = ConvertUtils.strToObject<TradeInfo>(sss[1].Trim());

            if (bout.tradeInfos[0] != null)
                bout.tradeInfos[0].Code = bout.code;
            if (bout.tradeInfos[1] != null)
                bout.tradeInfos[1].Code = bout.code;
            return bout;
        }
        #endregion


        #region 状态信息

        /// <summary>未开始</summary>         
        public int STATE_UNOPEN = 0;
        /// <summary>进行中</summary>         
        public int STATE_ONGOING = -1;
        /// <summary>已完成</summary>         
        public int STATE_COMPELETE = 1;
        /// <summary>
        /// 进行状态
        /// </summary>
        public int State
        {
            get
            {
                if (tradeInfos == null || tradeInfos.Length <= 0 || (tradeInfos[0] == null)) return STATE_UNOPEN;
                return Completed ? STATE_COMPELETE : STATE_ONGOING;
            }
        }
        /// <summary>
        /// 回合完成
        /// </summary>
        public bool Completed
        {
            get { return BuyInfo != null && SellInfo != null; }
        }

        /// <summary>
        /// 空头回合还是多头回合
        /// </summary>
        public TradeIntent ShortOrFull
        {
            get
            {
                if (tradeInfos == null || tradeInfos.Length <= 0 || tradeInfos[0] == null)
                    return TradeIntent.Unknown;
                return tradeInfos[0].Direction == TradeDirection.Buy ? TradeIntent.bull : TradeIntent.Short;
            }
        }
        /// <summary>
        /// 持仓天数
        /// </summary>
        public int PositionDays
        {
            get
            {
                if (BuyInfo == null || SellInfo == null)
                    return 0;
                return (int)Math.Abs((SellInfo.TradeDate-BuyInfo.TradeDate).TotalDays);
            }
        }
        /// <summary>
        /// 持仓时间（秒）
        /// </summary>
        public double PositionTimes
        {
            get
            {
                if (BuyInfo == null || SellInfo == null)
                    return 0;
                return (int)Math.Abs((BuyInfo.TradeDate - SellInfo.TradeDate).TotalSeconds);
            }
        }

        
        
        
        #endregion

        #region 交易信息

        /// <summary>
        /// 交易信息
        /// </summary>
        public TradeInfo[] TradeInfos { get { return tradeInfos; } }
        /// <summary>
        /// 买入信息
        /// </summary>
        public TradeInfo BuyInfo
        {
            get
            {
                if (tradeInfos == null) return null;
                foreach(TradeInfo tradeInfo in tradeInfos)
                {
                    if (tradeInfo != null && tradeInfo.Direction == TradeDirection.Buy)
                        return tradeInfo;
                }
                return null;
            }
        }

        /// <summary>
        /// 卖出信息
        /// </summary>
        public TradeInfo SellInfo
        {
            get
            {
                if (tradeInfos == null) return null;
                foreach (TradeInfo tradeInfo in tradeInfos)
                {
                    if (tradeInfo != null && tradeInfo.Direction == TradeDirection.Sell)
                        return tradeInfo;
                }
                return null;
            }
        }

        /// <summary>
        /// 记录交易
        /// </summary>
        /// <param name="num">建仓还是平仓（1或者2）</param>
        /// <param name="date">日期</param>
        /// <param name="direction">方向</param>
        /// <param name="price">价格</param>
        /// <param name="amount">数量</param>
        /// <param name="volumecommission">交易佣金</param>
        /// <param name="stampduty">印花税</param>
        public void RecordTrade(int num, DateTime date, TradeDirection direction, double price, int amount,double volumecommission,double stampduty,String reason="")
        {
            if (num != 1 && num != 2) return;
            this.tradeInfos[num - 1] = new TradeInfo()
            {
                Code = this.code,
                Direction = direction,
                TradeDate = date,
                Amount = amount,
                TradePrice = price,
                Fee = volumecommission,
                Stamps = stampduty,
                Reason = reason
            };
        }
        #endregion

        #region 盈利情况 
        /// <summary>
        /// 是否盈利（未计算费用）
        /// </summary>
        public bool Win
        {
            get { return Profit > 0; }
        }
        /// <summary>
        /// 盈利
        /// </summary>
        public double Profit
        {
            get
            {
                if (SellInfo == null || BuyInfo == null) return 0;
                return SellInfo.TradeCost - BuyInfo.TradeCost;
            }
        }
        /// <summary>
        
        /// <summary>
        /// 盈利率(盈利/买入花销)
        /// </summary>
        public double EarningsRate
        {
            get
            {
                return Profit / BuyInfo.TradeCost;
            }
        }
        
        public override string ToString()
        {
            if (State == STATE_UNOPEN) return "状态=未开始";
            else if (State == STATE_ONGOING) return tradeInfos[0].ToString();
            else return "盈利=" + Profit + ",盈利率=" + EarningsRate.ToString("F2") + ",持仓天数=" + PositionDays.ToString() + 
                        "(" + tradeInfos[0].ToString() + "---->" + tradeInfos[1].ToString() + ")";
        }
        #endregion

        #region 实现IComparable
        int IComparable<TradeBout>.CompareTo(TradeBout other)
        {
            if (other == null) return 1;

            if (this.State == STATE_UNOPEN && other.State == STATE_UNOPEN)
                return 0;
            else if (this.State == STATE_UNOPEN)
                return -1;
            else if (other.State == STATE_UNOPEN)
                return 1;
            return (int)(this.TradeInfos[0].TradeDate.Ticks - other.TradeInfos[0].TradeDate.Ticks);
        }
        #endregion


       
    }
}
