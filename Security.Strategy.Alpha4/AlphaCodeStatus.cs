using insp.Utility.Collections;
using insp.Utility.Date;
using insp.Utility.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Strategy.Alpha
{
    /// <summary>
    /// 执行阶段1
    /// </summary>
    public enum AlphaStage
    {
        /// <summary>
        /// 空闲
        /// </summary>
        Idle,
        /// <summary>
        /// 进入低位
        /// </summary>
        EnterLow,
        /// <summary>
        /// 已委托待买入
        /// </summary>
        EntrustBuy,
        /// <summary>
        /// 持仓中
        /// </summary>
        Hold,
        /// <summary>
        /// 已委托待卖出
        /// </summary>
        EntrustSell,
    }
    /// <summary>
    /// 执行状态
    /// </summary>
    public class AlphaCodeStatus
    {
        #region 基本信息
        /// <summary>
        /// 执行阶段名
        /// </summary>
        public static readonly List<String> STAGE_NAMES = CollectionUtils.AsList("空闲", "进入低位", "委托买入", "持仓", "委托卖出");
        
        /// <summary>
        /// 代码
        /// </summary>
        private String code;
        /// <summary>
        /// 代码
        /// </summary>
        public String Code { get { return code; } set { code = value; } }

        /// <summary>
        /// 所处阶段
        /// </summary>
        private AlphaStage stage = AlphaStage.Idle;
        /// <summary>
        /// 所处阶段
        /// </summary>
        public AlphaStage Stage { get { return stage; } set { stage = value; } }
        /// <summary>
        /// 所处阶段
        /// </summary>
        public String StageName { get { return STAGE_NAMES[(int)stage]; } set { stage = (AlphaStage)STAGE_NAMES.IndexOf(value); } }

        #endregion

        #region 关注阶段
        /// <summary>
        /// 进入低位日期
        /// </summary>
        private DateTime enterLowDate = DateUtils.InitDate;
        /// <summary>
        /// 进入低位日期
        /// </summary>
        public DateTime EnterLowDate { get { return enterLowDate; } set { enterLowDate = value; } }
        /// <summary>
        /// 进入低位日期
        /// </summary>
        public String EnterLowDateStr { get { return enterLowDate.ToString("yyyyMMdd"); } set { enterLowDate = DateTime.ParseExact(value, "yyyyMMd", null); } }
        /// <summary>
        /// 进入低位主力线值
        /// </summary>
        private double enterlowFundMain;
        /// <summary>
        /// 进入低位主力线值
        /// </summary>
        public double EnterlowFundMain { get { return enterlowFundMain; } set { enterlowFundMain = value; } }
        /// <summary>
        /// 进入低位主力线值
        /// </summary>
        public String EnterlowFundMainStr { get { return enterlowFundMain.ToString("F3"); } set { enterlowFundMain = double.Parse(value); } }

        #endregion

        #region 计划建仓阶段
        /// <summary>自动</summary>         
        public const String TRADE_AUTO = "自动";
        /// <summary>人工</summary>         
        public const String TRADE_MAUAL = "人工";
        /// <summary>
        /// 建仓交易方式
        /// </summary>
        private String setupTradeMode = TRADE_AUTO;
        /// <summary>
        /// 建仓交易方式
        /// </summary>
        public String SetupTradeMode { get { return setupTradeMode; } set { setupTradeMode = value; } }
        /// <summary>
        /// 计划建仓日期
        /// </summary>
        private DateTime setupDate = DateUtils.InitDate;
        /// <summary>
        /// 计划建仓日期
        /// </summary>
        public DateTime SetupDate { get { return setupDate; } set { setupDate = value; } }
        /// <summary>
        /// 计划建仓日期
        /// </summary>
        public String SetupDateStr { get { return setupDate.ToString("yyyyMMdd"); } set { setupDate = DateTime.ParseExact(value,"yyyyMMdd",null); } }
        /// <summary>
        /// 计划建仓日主力线值
        /// </summary>
        private double setupFundMain;
        /// <summary>
        /// 计划建仓日主力线值
        /// </summary>
        public double SetupFundMain { get { return setupFundMain; } set { setupFundMain = value; } }
        /// <summary>
        /// 计划建仓日主力线值
        /// </summary>
        public String SetupFundMainStr { get { return setupFundMain.ToString("F3"); } set { setupFundMain = double.Parse(value); } }
        /// <summary>
        /// 计划建仓日前一日主力线值
        /// </summary>
        private double setupPrevFundMain;
        /// <summary>
        /// 计划建仓日前一日主力线值
        /// </summary>
        public double SetupPrevFundMain { get { return setupPrevFundMain; } set { setupPrevFundMain = value; } }
        /// <summary>
        /// 计划建仓日前一日主力线值
        /// </summary>
        public String SetupPrevFundMainStr { get { return setupPrevFundMain.ToString("F3"); } set { setupPrevFundMain = double.Parse(value); } }
        /// <summary>
        /// 计划建仓价格
        /// </summary>
        private double setupPrice;
        /// <summary>
        /// 计划建仓价格
        /// </summary>
        public double SetupPrice { get { return setupPrice; } set { setupPrice = value; } }
        /// <summary>
        /// 计划建仓价格
        /// </summary>
        public String SetupPriceStr { get { return setupPrice.ToString("F2"); } set { setupPrice = double.Parse(value); } }
        /// <summary>
        /// 计划建仓数量
        /// </summary>
        private int setupAmount;
        /// <summary>
        /// 计划建仓数量
        /// </summary>
        public int SetupAmount { get { return setupAmount; } set { setupAmount = value; } }
        /// <summary>
        /// 计划建仓数量
        /// </summary>
        public String SetupAmountStr { get { return setupAmount.ToString(); } set { setupAmount = int.Parse(value); } }

        #endregion

        #region 持仓阶段
        /// <summary>
        /// 持仓阶段
        /// </summary>
        private int holdStage = 1;
        /// <summary>
        /// 持仓阶段
        /// </summary>
        public int HoldState { get { return holdStage; } set { holdStage = value; } }
        #endregion

        #region 计划平仓阶段
        /// <summary>
        /// 卖出交易方式
        /// </summary>
        private String closeTradeMode = TRADE_AUTO;
        /// <summary>
        /// 卖出交易方式
        /// </summary>
        public String CloseTradeMode { get { return closeTradeMode; } set { closeTradeMode = value; } }

        /// <summary>
        /// 计划平仓日期
        /// </summary>
        private DateTime closeDate;
        /// <summary>
        /// 计划平仓日期
        /// </summary>
        public DateTime CloseDate { get { return closeDate; } set { closeDate = value; } }
        /// <summary>
        /// 计划平仓日期
        /// </summary>
        public String CloseDateStr { get { return closeDate.ToString("yyyyMMdd"); } set { closeDate = DateTime.ParseExact(value, "yyyyMMdd", null); } }
        /// <summary>
        /// 平仓原因
        /// </summary>
        private String closeReason = "";
        /// <summary>
        /// 平仓原因
        /// </summary>
        public String CloseReason { get { return closeReason; } set { closeReason = value; } }

        /// <summary>
        /// 计划平仓价格
        /// </summary>
        private double closePrice;
        /// <summary>
        /// 计划平仓价格
        /// </summary>
        public double ClosePrice { get { return closePrice; } set { closePrice = value; } }
        /// <summary>
        /// 计划平仓价格
        /// </summary>
        public String ClosePriceStr { get { return closePrice.ToString("F2"); } set { closePrice = double.Parse(value); } }
        /// <summary>
        /// 计划平仓数量
        /// </summary>
        private int closeAmount;
        /// <summary>
        /// 计划平仓数量
        /// </summary>
        public int CloseAmount { get { return closeAmount; } set { closeAmount = value; } }
        /// <summary>
        /// 计划平仓数量
        /// </summary>
        public String CloseAmountStr { get { return closeAmount.ToString(); } set { closeAmount = int.Parse(value); } }

        #endregion

        

        #region 实际交易
        /// <summary>
        /// 交易回合
        /// </summary>
        private TradeBout tradeBout;
        /// <summary>
        /// 交易回合
        /// </summary>
        public TradeBout TradeBout { get { return tradeBout; } set { tradeBout = value; } }
        /// <summary>
        /// 交易回合
        /// </summary>
        public String TradeBoutStr { get { return tradeBout == null ? "" : tradeBout.ToString(); } set { tradeBout = TradeBout.Parse(value); } }

        #endregion


        #region 读写
        /// <summary>
        /// 取得标题
        /// </summary>
        /// <returns></returns>
        public static String GetTitle()
        {
            return "代码,状态,进入低位日期,进入低位主力值,建仓方式,计划建仓日期,计划建仓日主力值,计划建仓前一日值,计划建仓价格,计划建仓数量,持仓阶段,平仓方式,平仓原因,计划平仓价格,计划平仓数量,交易回合信息";
        }
        /// <summary>
        /// 字符串
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return Code + "," + StageName + "," + 
                   EnterLowDateStr + "," + EnterlowFundMainStr + "," + 
                   SetupTradeMode + "," + SetupDateStr + "," + SetupFundMainStr + "," + SetupPrevFundMainStr + "," + SetupPriceStr + "," + SetupAmount.ToString() + "," +
                   HoldState.ToString() + "," +
                   CloseTradeMode + "," + closeReason + "," + CloseDateStr + "," + ClosePriceStr + "," + CloseAmountStr + "," + TradeBoutStr;
        }
        /// <summary>
        /// 解析字符串
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static AlphaCodeStatus Parse(String s)
        {
            if (s == null || s.Trim() == "")
                return null;
            String[] ss = s.Trim().Split(',');
            if (ss == null || ss.Length < 16)
                return null;
            AlphaCodeStatus r = new AlphaCodeStatus();
            r.Code = ss[0] == null ? "" : ss[0].Trim();
            r.StageName = ss[1] == null ? "" : ss[1].Trim();
            r.EnterLowDateStr = ss[2] == null ? "" : ss[2].Trim();
            r.EnterlowFundMainStr = ss[3] == null ? "" : ss[3].Trim();
            r.SetupTradeMode = ss[4] == null ? "" : ss[4].Trim();
            r.SetupDateStr = ss[5] == null ? "" : ss[5].Trim();
            r.SetupFundMainStr = ss[6] == null ? "" : ss[6].Trim();
            r.SetupPrevFundMainStr = ss[7] == null ? "" : ss[7].Trim();
            r.SetupPriceStr = ss[8] == null ? "" : ss[8].Trim();
            r.SetupAmountStr = ss[9] == null ? "" : ss[9].Trim();
            r.HoldState = int.Parse(ss[10] == null ? "1" : ss[10].Trim());
            r.CloseTradeMode = ss[10] == null ? "" : ss[10].Trim();
            r.closeReason = ss[11] == null ? "" : ss[11].Trim();
            r.CloseDateStr = ss[12] == null ? "" : ss[12].Trim();
            r.ClosePriceStr = ss[13] == null ? "" : ss[13].Trim();

            String boutStr = StringUtils.Merge(ss, 14, ss.Length - 1);
            r.tradeBout = TradeBout.Parse(boutStr);
            return r;
        }
        #endregion
    }
}
