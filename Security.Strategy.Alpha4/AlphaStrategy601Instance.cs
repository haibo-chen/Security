using insp.Security.Data;
using insp.Security.Data.kline;
using insp.Utility.Bean;
using insp.Utility.Collections;
using insp.Utility.Collections.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Strategy.Alpha
{
    public class AlphaStrategy601Instance : StrategyInstance
    {
        #region 初始化
        /// <summary>
        /// 版本
        /// </summary>
        public override Version Version { get { return new Version(6, 0, 1); } }

        /// <summary>
        /// 构造方法
        /// </summary>
        internal AlphaStrategy601Instance(Properties props) { id = "1"; this.props = props; }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="id"></param>
        internal AlphaStrategy601Instance(String id, Properties props) { this.id = id; this.props = props; }

        #endregion

        #region 回测


        protected override List<TradeBout> doTestByCodes(List<string> codes)
        {
            if (codes == null || codes.Count <= 0)
                return new List<TradeBout>();

            IndicatorRepository repository = (IndicatorRepository)backtestParam.Get<Object>("repository");
            //取得策略参数
            double buy_mainlow = props.Get<double>("buy_day_mainlow"); //主力线低位买入
            int buy_cross = props.Get<int>("buy_day_corss");           //主力线金叉买入

            int sell_maxholddays = props.Get<int>("sell_maxholddays");//最大持仓天数

            int sell_notrun_num = props.Get<int>("sell_notrun_num");//主力线与价格趋势不符允许出现的最大次数
            int sell_selectnum = props.Get<int>("sell_selectnum");//可以尝试的最大卖出次数

            double sell_mainvalve = props.Get<double>("sell_mainvalve");//主力线高位阈值
            double sell_mainvalve_diff = props.Get<double>("sell_mainvalve_diff");//主力线高位增幅
            

            double sell_slopediff = props.Get<double>("sell_slopediff");   //主力线和收盘价的斜率差阈值
            sell_slopediff = (sell_slopediff / 180) * Math.PI;
            double sell_slopepoint = props.Get<double>("sell_slopepoint"); //线性回归斜率的卖点
            sell_slopepoint = (sell_slopepoint / 180) * Math.PI;
            GetInMode p_getinMode = (GetInMode)props.Get<GetInMode>("fundpergetin");

            List<TradeBout> allbouts = new List<TradeBout>();
               
            foreach (String code in codes)
            {
                #region 买入条件判断         
                List<TradeBout> bouts = new List<TradeBout>();
                TimeSerialsDataSet ds = repository[code];
                if (ds == null) continue;

                KLine kline = ds.DayKLine;
                if (kline == null) continue;
                TimeSeries<ITimeSeriesItem<List<double>>> dayFunds = ds.DayFundTrend;
                TimeSeries<ITimeSeriesItem<double>> dayFundsCross = ds.DayFundTrendCross;

                if (buy_cross == 0 && dayFunds == null)
                    continue;
                else if (buy_cross == 1 && (dayFundsCross == null || dayFunds == null))
                    continue;
                #region 判断主力线低位决定买入点
                if (buy_cross == 0)
                {
                    for (int i = 0; i < dayFunds.Count; i++)
                    {
                        if (dayFunds[i].Date.Date < backtestParam.BeginDate || dayFunds[i].Date.Date > backtestParam.EndDate)
                            continue;
                        if (double.IsNaN(dayFunds[i].Value[0]))
                            continue;
                        if (dayFunds[i].Value[0] > buy_mainlow)
                            continue;
                        //主力线开始低于buy_mainlow...
                        i += 1;
                        while (i < dayFunds.Count)
                        {
                            if (dayFunds[i].Value[0] <= buy_mainlow)
                            {
                                i += 1;
                                continue;
                            }
                            //主力线出了buy_mainlow
                            KLineItem klineItem = kline[dayFunds[i].Date];
                            if (klineItem == null) break;
                            int tIndex = kline.IndexOf(klineItem);
                            if (tIndex >= kline.Count - 1) break;
                            KLineItem klineItemNext = kline[tIndex + 1];
                            TradeBout bout = new TradeBout(code);
                            double price = klineItem.CLOSE;
                            if (price > klineItemNext.HIGH || price < klineItemNext.LOW)
                                break;
                            bout.RecordTrade(1, dayFunds[i].Date.Date, TradeDirection.Buy, price, (int)(p_getinMode.Value / price), backtestParam.volumecommission, backtestParam.stampduty, "主力线低于" + buy_mainlow.ToString("F2"));
                            bouts.Add(bout);
                            break;
                        }
                    }
                }
                #endregion

                #region 判断金叉决定买入点
                else if (buy_cross == 1)
                {
                    for (int i = 0; i < dayFundsCross.Count; i++)
                    {
                        if (dayFundsCross[i].Date.Date < backtestParam.BeginDate || dayFundsCross[i].Date.Date > backtestParam.EndDate)
                            continue;
                        if (dayFundsCross[i].Value <= 0)
                            continue;
                        ITimeSeriesItem<List<double>> dayFundItem = dayFunds[dayFundsCross[i].Date];
                        if (dayFundItem == null) continue;
                        if (buy_mainlow != 0 && dayFundItem.Value[0] >= buy_mainlow) continue;

                        KLineItem klineItem = kline[dayFundItem.Date];
                        if (klineItem == null) continue;
                        int tIndex = kline.IndexOf(klineItem);
                        if (tIndex >= kline.Count - 1) continue;
                        KLineItem klineItemNext = kline[tIndex + 1];
                        TradeBout bout = new TradeBout(code);
                        double price = klineItem.CLOSE;
                        if (price > klineItemNext.HIGH || price < klineItemNext.LOW)
                            continue;
                        bout.RecordTrade(1, dayFunds[i].Date.Date, TradeDirection.Buy, price, (int)(p_getinMode.Value / price), backtestParam.volumecommission, backtestParam.stampduty, "主力线低于" + buy_mainlow.ToString("F2"));
                        bouts.Add(bout);
                    }
                }
                #endregion

                #endregion

                #region 卖出条件判断
                for (int i = 0; i < bouts.Count; i++)
                {
                    //取得时序数据
                    TradeBout bout = bouts[i];                   
                    
                    DateTime buyDate = bout.BuyInfo.TradeDate;//买入日期
                    DateTime d = buyDate.AddDays(2);
                    int days = 2; //买入后的第几天

                    double prevMainFundValue = 0;//前一日的主力值
                    double mainFunddiff = 0;//主力线当日与前一日的差值                
                    int is_slope_run = 0;//主力线和收盘价走势是否一致，0未知；1一致；-1，-2不一致
                    int sellnum = 0; //择机卖出次数

                    int state = 0;   //当日状态；0未知；1 择机卖出(在连续sell_selectnum内只要不亏损就卖)
                    String stateReason = "";//卖出原因
                    while (d <= backtestParam.EndDate)
                    {
                        //查找d日的资金线,找不到则跳过这天
                        int dayFundIndex = dayFunds.IndexOf(d);
                        if (dayFundIndex < 0)
                        {
                            d = d.AddDays(1);
                            continue;
                        }
                        ITimeSeriesItem<List<double>> dayFundsItem = dayFunds[dayFundIndex];
                        //查找当日K线,找不到则跳过这天
                        int dayKLineIndex = kline.IndexOf(d);
                        if (dayKLineIndex < 0)
                        {
                            d = d.AddDays(1);
                            continue;
                        }
                        KLineItem dayLineItem = kline[dayKLineIndex];



                        //计算以d日收盘价卖出的盈利情况
                        bout.RecordTrade(2, d, TradeDirection.Sell, kline[dayKLineIndex].CLOSE, bout.BuyInfo.Amount, backtestParam.volumecommission, backtestParam.stampduty);
                        double earnRate = bout.EarningsRate;
                        bout.TradeInfos[1] = null;

                        //如果是择机卖出状态
                        if (state == 1)
                        {
                            if (sellnum > sell_selectnum || earnRate > 0)
                            {
                                bout.RecordTrade(2, d, TradeDirection.Sell, kline[dayKLineIndex].CLOSE, bout.BuyInfo.Amount, backtestParam.volumecommission, backtestParam.stampduty, stateReason + ",延迟天数=" + sellnum.ToString());
                                break;
                            }
                            sellnum += 1;
                            d = d.AddDays(1);
                            days += 1;
                            continue;
                        }

                        //如果超过最大持仓天数，则进入到择机卖出
                        if (days >= sell_maxholddays)
                        {
                            state = 1;
                            continue;
                        }

                        //趋势不一致出现sell_notrun_num次，择机卖出
                        if (is_slope_run <= -1 * sell_notrun_num)
                        {
                            stateReason = "主力线趋势不符" + is_slope_run.ToString() + "次数";
                            state = 1;
                            continue;
                        }
                        //主力线超出预定值
                        if (sell_mainvalve != 0 && dayFunds[dayFundIndex].Value[0] >= sell_mainvalve)
                        {
                            if (prevMainFundValue == 0)
                            {
                                prevMainFundValue = dayFunds[dayFundIndex].Value[0];
                                d = d.AddDays(1);
                                days += 1;
                                continue;
                            }
                            else if ((dayFunds[dayFundIndex].Value[0] - prevMainFundValue) > sell_mainvalve_diff)
                            {
                                d = d.AddDays(1);
                                days += 1;
                                continue;
                            }
                            mainFunddiff = (dayFunds[dayFundIndex].Value[0] - prevMainFundValue);
                            //如果盈利率小于0，延迟数天卖出
                            if (earnRate <= 0)
                            {
                                state = 1;
                                stateReason = "主力线突破高位且增幅减缓" + mainFunddiff.ToString("F3");
                                continue;

                            }
                            //卖出操作
                            bout.RecordTrade(2, d, TradeDirection.Sell, dayLineItem.CLOSE, bout.BuyInfo.Amount, backtestParam.volumecommission, backtestParam.stampduty, "主力线突破高位且增幅减缓" + mainFunddiff.ToString("F3"));
                            break;

                        }
                        //计算线性回归斜率
                        int begin = dayFunds.IndexOf(buyDate);
                        int end = dayFundIndex;
                        List<double> list1 = new List<double>();
                        for (int k = begin; k <= end; k++)
                            list1.Add(dayFunds[k].Value[0]);
                        double fundT = list1.Normalization().SLOPE();

                        begin = kline.IndexOf(buyDate);
                        end = dayKLineIndex;
                        List<double> list2 = new List<double>();
                        for (int k = begin; k <= end; k++)
                            list2.Add(kline[k].CLOSE);
                        double closeT = list2.Normalization().SLOPE();

                        //log.Info("斜率=" + fundT.ToString("F3") + "-" + closeT.ToString("F3") + "=" + Math.Abs(fundT - closeT).ToString("F3"));
                        //两个线性回归斜率不一致
                        if (Math.Abs(fundT - closeT) >= sell_slopediff)
                        {
                            is_slope_run -= 1;
                        }else //两个线性回归斜率一致
                        {
                            if(fundT <= sell_slopepoint)
                            {
                                KLineItem prevKlineItem = kline[dayKLineIndex - 1];
                                if(prevKlineItem.CLOSE > dayLineItem.CLOSE)//价格下降了
                                {
                                    if (earnRate > 0)
                                    {
                                        bout.RecordTrade(2, d, TradeDirection.Sell, dayLineItem.CLOSE, bout.BuyInfo.Amount, backtestParam.volumecommission, backtestParam.stampduty, "斜率一致且增幅小于阈值(" + fundT.ToString("F2") + "<" + sell_slopepoint.ToString("F2"));
                                        break;
                                    }
                                }
                                /*if(earnRate>0)
                                {
                                    bout.RecordTrade(2, d, TradeDirection.Sell, dayLineItem.CLOSE, bout.BuyInfo.Amount, backtestParam.volumecommission, backtestParam.stampduty, "斜率一致且增幅小于阈值("+ fundT.ToString("F2")+"<"+ sell_slopepoint.ToString("F2"));
                                    break;
                                }*/
                            }
                        }
                        //进入下一天
                        d = d.AddDays(1);
                        days += 1;
                        continue;
                    }
                }
                #endregion

                //最后删除未完成的回合
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

                    /*foreach(TradeBout bout in bouts)
                    {
                        log.Info("  " + bout.ToString());
                    }*/

                }
            }
         

           

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
