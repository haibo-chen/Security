using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Bean;
using insp.Security.Data;
using insp.Security.Data.kline;
using insp.Utility.Collections.Time;

namespace insp.Security.Strategy.Alpha.Sell
{
    public class DoSellMaxProfilt : Seller
    {
        /// <summary>
        /// 判断指定的回合是否要在d这天卖出
        /// </summary>
        /// <param name="code"></param>
        /// <param name="bout"></param>        
        /// <param name="d"></param>
        /// <param name="strategyParam"></param>
        /// <param name="backtestParam"></param>
        /// <returns>是否进入择机卖出</returns>
        public override bool DoSell(String code, TradeBout bout, DateTime d, Properties strategyParam, BacktestParameter backtestParam, out String reason)
        {
            reason = "";
            return false;
        }

        public override TradeInfo DoSell(HoldRecord holdRecord, DateTime d, Properties strategyParams, StrategyContext context)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 执行卖出操作
        /// </summary>
        /// <param name="tradeRecord"></param>
        /// <param name="strategyParam"></param>
        /// <param name="backtestParam"></param>
        public override void Execute(TradeRecords tradeRecord, Properties strategyParam, BacktestParameter backtestParam)
        {
            //初始化行情库
            if (tradeRecord == null)
                return;

            IndicatorRepository repository = (IndicatorRepository)backtestParam.Get<Object>("repository");
            if (repository == null)
                return;

            //取得策略参数
            int p_maxday = strategyParam.Get<int>("maxholddays");

            //遍历每一个买入回合
            String code = tradeRecord.Code;
            List<TradeBout> bouts = tradeRecord.Bouts;
            for (int i = 0; i < bouts.Count; i++)
            {               
                TradeBout bout = bouts[i];
                TimeSerialsDataSet ds = repository[bout.Code];
                if (ds == null) continue;
                if (bout.Completed) continue;//跳过已完成的
                KLine kline = ds.DayKLine;
                if (kline == null) continue;

                int bIndex = kline.IndexOf(bout.BuyInfo.TradeDate);

                //寻找p_maxday天内最大收益那天
                KLineItem maxProfileItem = null;
                for(int index=bIndex+1;index<=bIndex+1+p_maxday;index++)
                {
                    if (index >= kline.Count) break;
                    KLineItem item = kline[index];
                    if(item.CLOSE > (maxProfileItem==null?0: maxProfileItem.CLOSE))
                    {
                        maxProfileItem = item;
                    }
                }
                if (maxProfileItem == null)
                    continue;
                bout.RecordTrade(2, maxProfileItem.Date, TradeDirection.Sell, maxProfileItem.CLOSE, bout.BuyInfo.Amount, backtestParam.Volumecommission, backtestParam.Stampduty, "");
            }
        }
    }
}