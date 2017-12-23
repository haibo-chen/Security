using insp.Security.Data;
using insp.Security.Data.kline;
using insp.Utility.Date;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Strategy
{
    /// <summary>
    /// 执行统计 
    /// </summary>
    public class TotalStat
    {
        #region 统计项目
        /// <summary>
        /// 交易股数
        /// </summary>
        public int Count;
        /// <summary>
        /// 回合数
        /// </summary>
        public int BoutNum;
        /// <summary>
        /// 胜数
        /// </summary>
        public int WinNum;
        /// <summary>
        /// 胜率
        /// </summary>
        public double WinRate;
        /// <summary>
        /// 胜率
        /// </summary>
        public String WinRateStr { get { return WinRate.ToString("F2"); } }
        /// <summary>
        /// 收益总资产
        /// </summary>
        public double TotalFund;
        /// <summary>
        ///  收益总资产
        /// </summary>
        public String TotalFundStr { get { return TotalFund.ToString("F2"); } }
        /// <summary>
        /// 收益率
        /// </summary>
        public double TotalProfilt;
        /// <summary>
        /// 收益率
        /// </summary>
        public String TotalProfiltStr { get { return TotalProfilt.ToString("F2"); } }
        /// <summary>
        /// 平均持仓天数
        /// </summary>
        public int AvgHoldDays;
        /// <summary>
        /// 最大持仓天数
        /// </summary>
        public int MaxHoldDays;
        /// <summary>
        /// 持仓天数(平均/最大)
        /// </summary>
        public String HoldDays
        {
            get {
                if (AvgHoldDays == 0 && MaxHoldDays == 0)
                    return "";
                return AvgHoldDays.ToString() + "/" + MaxHoldDays.ToString();
            }
        }
        /// <summary>
        /// 最大回撤率
        /// </summary>
        public double MaxRetracementRate;
        /// <summary>
        /// 最大回撤日期
        /// </summary>
        public DateTime MaxRetracementDate;

        /// <summary>
        /// 回撤率(最大[日期])
        /// </summary>
        public String MaxRetracement
        {
            get
            {
                if (MaxRetracementRate == 0)
                    return "0";
                return MaxRetracementRate.ToString("F3") + "[" + MaxRetracementDate.ToString("yyyyMMdd") + "]";
            }
        }

        /// <summary>
        /// (初始)最大回撤率
        /// </summary>
        public double MaxInitRetracementRate;
        /// <summary>
        /// (初始)最大回撤日期
        /// </summary>
        public DateTime MaxInitRetracementDate;
        /// <summary>
        /// (初始)回撤率(平均/最大)
        /// </summary>
        public String InitRetracement
        {
            get
            {
                if (MaxInitRetracementRate == 0)
                    return "0";
                return MaxInitRetracementRate.ToString("F5")+"[" + MaxInitRetracementDate.ToString("yyyyMMdd")+"]";
            }
        }
        /// <summary>
        /// 平均每天交易次数
        /// </summary>
        public double AverageTradeCountPerDay;
        /// <summary>
        /// 最大一条交易次数
        /// </summary>
        public int MaxTradeCountPerDay;

        /// <summary>
        /// 每天交易次数
        /// </summary>
        public String TradeCountPerDay
        {
            get
            {
                return AverageTradeCountPerDay.ToString("F2") + "/" + MaxTradeCountPerDay.ToString();
            }
        }
        #endregion

        #region 详细记录
        /// <summary>
        /// 详细记录
        /// </summary>
        protected List<DateDetailRecord> records = new List<DateDetailRecord>();
        /// <summary>
        /// 详细记录
        /// </summary>
        public List<DateDetailRecord> Records { get { return records; } set { records = value; } }
        #endregion

        #region 初始化
        /// <summary>
        /// 构造方法
        /// </summary>
        public TotalStat()
        {

        }
        
        #endregion

        #region 显示
        /// <summary>
        /// 总结
        /// </summary>
        /// <param name="log"></param>
        public void Summary(log4net.ILog log=null)
        {
            if(log == null)
                log = log4net.LogManager.GetLogger(typeof(TotalStat));
            log.Info("回测结果统计：" + System.Environment.NewLine + ToString());
           
            /*log.Info("回测结果统计：");
            log.Info("回测股票数=" + Count);
            log.Info("交易回合数=" + BoutNum);
            log.Info("胜率=" + WinRate.ToString("F2"));
            log.Info("累积资金=" + TotalFund.ToString("F2"));
            log.Info("累积盈利率=" + TotalProfilt.ToString("F2"));
            log.Info("持仓天数=" + HoldDays);
            log.Info("回撤率=" + Retracementrate);
            log.Info("日交易次数 =" + TradeCountPerDay);*/
        }
        /// <summary>
        /// 字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "回测股票数=" + Count + System.Environment.NewLine +
                   "交易回合数=" + BoutNum + System.Environment.NewLine +
                   "胜率=" + WinRate.ToString("F2") + System.Environment.NewLine +
                   "累积资金=" + TotalFund.ToString("F2") + System.Environment.NewLine +
                   "累积盈利率=" + TotalProfilt.ToString("F2") + System.Environment.NewLine +
                   "持仓天数=" + HoldDays + System.Environment.NewLine +
                   "回撤率=" + MaxRetracement + System.Environment.NewLine +
                   "(初始)回撤率=" + InitRetracement + System.Environment.NewLine +
                   "日交易次数 =" + TradeCountPerDay;
        }
        /// <summary>
        /// 取得标题
        /// </summary>
        /// <returns></returns>
        public static String GetTitle()
        {
            return "股票数,回合数,胜率,收益率,总资产,持仓天数(平均/最长),回撤率,日交易次数(平均/最大)";
        }
        /// <summary>
        /// 取得标题值
        /// </summary>
        /// <returns></returns>
        public String GetTitleValue()
        {
            return Count.ToString() + "," +
                   BoutNum.ToString() + "," +
                   WinRate.ToString("F2") + "," +
                   TotalProfilt.ToString("F2") + "," +
                   TotalFund.ToString("F2") + "," +
                   HoldDays + "," +
                   MaxRetracement + "," +
                   TradeCountPerDay;
        }
        #endregion

        #region 写文件
        
        /// <summary>
        /// 写详细记录
        /// </summary>
        /// <param name="recordFileName">每天的交易摘要文件</param>
        /// <param name="boutFileName">每天的交易明细文件</param>
        public void WriteRecord(String recordFileName,String boutFileName)
        {
            List<String> recordLines = new List<string>();
            List<String> boutLines = new List<string>();

            recordLines.Add(DateDetailRecord.GetTitle());
            for (int i=0;i<this.records.Count;i++)
            {
                recordLines.Add(records[i].ToString());
                boutLines.AddRange(records[i].ToDetailString());
            }
            if(recordLines.Count>0 && recordFileName != null && recordFileName != "")
                System.IO.File.WriteAllLines(recordFileName, recordLines.ToArray());
            if(boutLines.Count>0 && boutFileName != null && boutFileName != "")
                System.IO.File.WriteAllLines(boutFileName, boutLines.ToArray());
        }
        #endregion


    }
}   
