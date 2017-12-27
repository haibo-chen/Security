using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using insp.Utility.Bean;
using insp.Security.Data;
using insp.Utility.Collections.Time;
using insp.Security.Data.kline;
using insp.Utility.Collections;

namespace insp.Security.Strategy.Alpha.Sell
{
    /// <summary>
    /// 根据每天的经验买入
    /// </summary>
    public class DoSellerSense : Seller
    {
        public override void Execute(List<TradeBout> bouts, Properties strategyParam, BacktestParameter backtestParam)
        {
            if (bouts == null || bouts.Count <= 0)
                return;

            IndicatorRepository repository = (IndicatorRepository)backtestParam.Get<Object>("repository");
            if (repository == null)
                return;
            
            //取得策略参数
            
            int sell_maxholddays = strategyParam.Get<int>("sell_maxholddays");//最大持仓天数

            int sell_notrun_num = strategyParam.Get<int>("sell_notrun_num");//主力线与价格趋势不符允许出现的最大次数
            int sell_selectnum = strategyParam.Get<int>("sell_selectnum");//可以尝试的最大卖出次数

            double sell_mainvalve = strategyParam.Get<double>("sell_mainvalve");//主力线高位阈值
            double sell_mainvalve_diff = strategyParam.Get<double>("sell_mainvalve_diff");//主力线高位增幅


            double sell_slopediff = strategyParam.Get<double>("sell_slopediff");   //主力线和收盘价的斜率差阈值
            sell_slopediff = (sell_slopediff / 180) * Math.PI;
            double sell_slopepoint = strategyParam.Get<double>("sell_slopepoint"); //线性回归斜率的卖点
            sell_slopepoint = (sell_slopepoint / 180) * Math.PI;
            GetInMode p_getinMode = (GetInMode)strategyParam.Get<GetInMode>("getinMode");


            #region 卖出条件判断
            for (int i = 0; i < bouts.Count; i++)
            {
                //取得回合数据
                TradeBout bout = bouts[i];
                if (bout == null) continue;

                //取得时序数据
                TimeSerialsDataSet ds = repository[bout.Code];
                if (ds == null) continue;

                KLine kline = ds.DayKLine;
                TimeSeries<ITimeSeriesItem<List<double>>> dayFunds = ds.DayFundTrend;
                TimeSeries<ITimeSeriesItem<double>> dayFundsCross = ds.DayFundTrendCross;


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


                    //对买入后的每一天
                    //1.计算以d日收盘价卖出的盈利情况
                    bout.RecordTrade(2, d, TradeDirection.Sell, kline[dayKLineIndex].CLOSE, bout.BuyInfo.Amount, backtestParam.Volumecommission, backtestParam.Stampduty);
                    double earnRate = bout.EarningsRate;
                    bout.TradeInfos[1] = null;

                    //如果是择机卖出状态
                    if (state == 1)
                    {
                        if (sellnum > sell_selectnum || earnRate > 0)
                        {
                            bout.RecordTrade(2, d, TradeDirection.Sell, kline[dayKLineIndex].CLOSE, bout.BuyInfo.Amount, backtestParam.Volumecommission, backtestParam.Stampduty, stateReason + ",延迟天数=" + sellnum.ToString());
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
                        bout.RecordTrade(2, d, TradeDirection.Sell, dayLineItem.CLOSE, bout.BuyInfo.Amount, backtestParam.Volumecommission, backtestParam.Stampduty, "主力线突破高位且增幅减缓" + mainFunddiff.ToString("F3"));
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
                    }
                    else //两个线性回归斜率一致
                    {
                        if (fundT <= sell_slopepoint)
                        {
                            KLineItem prevKlineItem = kline[dayKLineIndex - 1];
                            if (prevKlineItem.CLOSE > dayLineItem.CLOSE)//价格下降了
                            {
                                if (earnRate > 0)
                                {
                                    bout.RecordTrade(2, d, TradeDirection.Sell, dayLineItem.CLOSE, bout.BuyInfo.Amount, backtestParam.Volumecommission, backtestParam.Stampduty, "斜率一致且增幅小于阈值(" + fundT.ToString("F2") + "<" + sell_slopepoint.ToString("F2"));
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

        }
    }
}
