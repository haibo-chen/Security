using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Security.Strategy;

namespace insp.Security.Strategy
{
    public class DateDetailRecord
    {
        #region 基本信息
        /// <summary>日期</summary>             
        public DateTime date;
        /// <summary>资金</summary>             
        public double curFund;
        /// <summary>最小市值</summary>             
        public double marketValueMin;
        /// <summary>最大市值</summary>             
        public double marketValueMax;
        /// <summary>可买数目</summary>             
        public int willBuyCount;
        /// <summary>实际买入数</summary>             
        public int buyCount;
        /// <summary>卖出数量</summary>             
        public int SellCount;
        /// <summary>持仓数量</summary>             
        public int holdCount;
        /// <summary>回撤率</summary>             
        public double retracement;

        /// <summary>
        /// 取得标题
        /// </summary>
        /// <returns></returns>
        public static String GetTitle()
        {
            return "日期,资金,最小市值,最大市值,可买数目,实际买入,卖出数量,持仓数量,回撤率";
        }
        /// <summary>
        /// 返回值
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return date.ToString("yyyyMMdd") + "," +
                   curFund.ToString("F2") + "," +
                   marketValueMin.ToString("F2") + "," +
                   marketValueMax.ToString("F2") + "," +
                   willBuyCount.ToString() + "," +
                   buyCount.ToString() + "," +
                   SellCount.ToString() + "," +
                   holdCount.ToString() + "," +
                   retracement.ToString();
        }
        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns></returns>
        public DateDetailRecord Clone()
        {
            DateDetailRecord r = new DateDetailRecord();
            r.buyBouts.AddRange(this.buyBouts);
            r.buyCount = this.buyCount;
            r.curFund = this.curFund;
            r.date = this.date;
            r.holdBouts.AddRange(this.holdBouts);
            r.marketValueMax = marketValueMax;
            r.marketValueMin = marketValueMin;
            r.retracement = retracement;
            r.sellBouts.AddRange(this.sellBouts);
            r.SellCount = SellCount;
            r.willBuyCount = willBuyCount;
            return r;
        }
        #endregion

        #region 详细信息
        /// <summary>
        /// 买入回合
        /// </summary>
        public List<TradeBout> buyBouts = new List<TradeBout>();
        /// <summary>
        /// 卖出回合
        /// </summary>
        public List<TradeBout> sellBouts = new List<TradeBout>();
        /// <summary>
        /// 持仓回合
        /// </summary>
        public List<String> holdBouts = new List<String>();
        /// <summary>
        /// 显示详细信息
        /// </summary>
        /// <returns></returns>
        public List<String> ToDetailString()
        {
            List<String> str = new List<string>();
            buyBouts.ForEach(x => str.Add(date.ToString("yyyyMMdd") + ",买入," + x.ToText()));
            sellBouts.ForEach(x => str.Add(date.ToString("yyyyMMdd") + ",卖出," + x.ToText()));
            holdBouts.ForEach(x => str.Add(date.ToString("yyyyMMdd") + ",持仓," + x));
            return str;
        }
        #endregion
    }
}
