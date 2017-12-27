using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Security.Data;
using insp.Security.Data.kline;
using insp.Utility.Bean;
using insp.Utility.Collections;
using insp.Utility.Collections.Time;
using insp.Security.Data.Indicator.Macd;
using insp.Security.Strategy.Alpha.Sell;

namespace insp.Security.Strategy.Alpha
{
    public class AlphaStrategy7Instance : StrategyInstance
    {
        #region 初始化
        /// <summary>
        /// 版本
        /// </summary>
        public override Version Version { get { return new Version(6, 0, 2); } }

        /// <summary>
        /// 构造方法
        /// </summary>
        internal AlphaStrategy7Instance(Properties props) { id = "1"; this.props = props; }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="id"></param>
        internal AlphaStrategy7Instance(String id, Properties props) { this.id = id; this.props = props; }

        #endregion

        #region 回测


        protected override List<TradeBout> doTestByCodes(List<string> codes)
        {
            if (codes == null || codes.Count <= 0)
                return new List<TradeBout>();

            IndicatorRepository repository = (IndicatorRepository)backtestParam.Get<Object>("repository");
            
            List<TradeBout> allbouts = new List<TradeBout>();

            DoBuyerMACDCross buyer = new DoBuyerMACDCross();
            DoSellerStage seller = new DoSellerStage();
            foreach (String code in codes)
            {
                TimeSerialsDataSet ds = repository[code];
                if (ds == null) continue;
                //买入
                List<TradeBout> bouts = buyer.Execute(code, props, backtestParam);
                if (bouts == null || bouts.Count <= 0)
                    continue;
                //卖出                
                seller.Execute(bouts, props, backtestParam);
                //删除剩余的
                RemoveUnCompeletedBouts(bouts);
                if (bouts != null && bouts.Count > 0)
                    allbouts.AddRange(bouts);

                ///打印
                if (bouts != null && bouts.Count > 0)
                {
                    double totalProfilt = allbouts.Sum(x => x.Profit);
                    double totalCost = allbouts.Sum(x => x.BuyInfo.TradeCost);
                    log.Info(ds.Code + ":回合数=" + bouts.Count.ToString() +
                                       ",胜率=" + (bouts.Count(x => x.Win) * 1.0 / bouts.Count).ToString("F2") +
                                       ",盈利=" + bouts.Sum(x => x.Profit).ToString("F2") +
                                       ",总胜率=" + (allbouts.Count(x => x.Win) * 1.0 / allbouts.Count).ToString("F3") +
                                       ",总盈利=" + totalProfilt.ToString("F2") +
                                       ",平均盈利率=" + (totalProfilt / totalCost).ToString("F3"));
                    //foreach(TradeBout bout in bouts)
                    //{
                    //    log.Info("  " + bout.ToString());
                    //}

                }
            }

            //seller.test(allbouts, props, backtestParam);
            return allbouts;
        }

        protected override bool isForbidBuy(DateTime d, string code, out string reason)
        {
            reason = "";
            return false;
        }

        protected override bool isForbidHold(DateTime d, string code, out string reason)
        {
            reason = "";
            return false;
        }
        #endregion

        #region 实际运行

        public override void Run(Properties context)
        {
            throw new NotImplementedException();
        }

        public override void DoAction(IStrategyContext context, EventArgs args)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
