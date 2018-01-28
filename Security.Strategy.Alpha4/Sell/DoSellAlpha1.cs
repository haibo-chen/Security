using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Bean;
using insp.Security.Data;
using insp.Security.Data.kline;
using insp.Utility.Collections.Time;
using insp.Security.Data.Indicator;
using insp.Utility.Date;

namespace insp.Security.Strategy.Alpha.Sell
{
    public class DoSellAlpha1 : Seller
    {
        public override TradeInfo DoSell(HoldRecord holdRecord, DateTime d, Properties strategyParam, StrategyContext context)
        {
            if (holdRecord == null)
                return null;

            //取得行情库
            IndicatorRepository repository = (IndicatorRepository)context.Get<Object>("repository");
            if (repository == null) return null;
            TimeSerialsDataSet ds = repository[holdRecord.code];
            if (ds == null) return null;
            KLine klineDay = ds.DayKLine;
            KLineItem klineItemDay = klineDay[d];
            if (klineItemDay == null) return null;

            //取得策略参数
            double p_maxprofilt = strategyParam.Get<double>("maxprofilt");
            int p_maxholddays = strategyParam.Get<int>("maxholddays");
            double p_stoploss = strategyParam.Get<double>("stoploss");
            int p_choosedays = strategyParam.Get<int>("choosedays");
            double p_chooseprofilt = strategyParam.Get<double>("chooseprofilt");
            GrailParameter p_grail = GrailParameter.Parse(strategyParam.Get<String>("grail"));
            double stampduty = context.Get<double>("stampduty");
            double volumecommission = context.Get<double>("volumecommission");

            //大盘要求必须卖
            if (p_grail.MustSell(d,holdRecord.code))
            {
                TradeInfo tradeInfo = new TradeInfo()
                {
                    Direction = TradeDirection.Sell,
                    Code = holdRecord.code,
                    Amount = holdRecord.amount,
                    EntrustPrice = klineItemDay.CLOSE,
                    EntrustDate = d,
                    TradeDate = d,
                    TradePrice = klineItemDay.CLOSE,
                    Stamps = stampduty,
                    Fee = volumecommission,
                    TradeMethod = TradeInfo.TM_AUTO,
                    Reason = "大盘指数要求卖出"
                };
                return tradeInfo;
            }

            double profilt = (klineItemDay.CLOSE - holdRecord.buyPrice) / holdRecord.buyPrice;
            //判断是否到达最大收益
            if (p_maxprofilt > 0)
            {                
                if(profilt >= p_maxprofilt)
                {
                    TradeInfo tradeInfo = new TradeInfo()
                    {
                        Direction = TradeDirection.Sell,
                        Code = holdRecord.code,
                        Amount = holdRecord.amount,
                        EntrustPrice = klineItemDay.CLOSE,
                        EntrustDate = d,
                        TradeDate = d,
                        TradePrice = klineItemDay.CLOSE,
                        Stamps = stampduty,
                        Fee = volumecommission,
                        TradeMethod = TradeInfo.TM_AUTO,
                        Reason = "盈利达到"+ p_maxprofilt.ToString("F2")
                    };
                    return tradeInfo;
                }
            }

            //盈利超过个股预期
            if(holdRecord.expect > 0)
            {
                
                if (profilt >= p_maxprofilt)
                {
                    TradeInfo tradeInfo = new TradeInfo()
                    {
                        Direction = TradeDirection.Sell,
                        Code = holdRecord.code,
                        Amount = holdRecord.amount,
                        EntrustPrice = klineItemDay.CLOSE,
                        EntrustDate = d,
                        TradeDate = d,
                        TradePrice = klineItemDay.CLOSE,
                        Stamps = stampduty,
                        Fee = volumecommission,
                        TradeMethod = TradeInfo.TM_AUTO,
                        Reason = "盈利达到个股预期" + holdRecord.expect.ToString("F2")
                    };
                    return tradeInfo;
                }
            }

            //预期要求立即卖出
            if (holdRecord.expect == -1)
            {
                TradeInfo tradeInfo = new TradeInfo()
                {
                    Direction = TradeDirection.Sell,
                    Code = holdRecord.code,
                    Amount = holdRecord.amount,
                    EntrustPrice = klineItemDay.CLOSE,
                    EntrustDate = d,
                    TradeDate = d,
                    TradePrice = klineItemDay.CLOSE,
                    Stamps = stampduty,
                    Fee = volumecommission,
                    TradeMethod = TradeInfo.TM_AUTO,
                    Reason = "个股预期极低"
                };
                return tradeInfo;
            }

            //个股观望天数超过预期
            int holdDays = CalendarUtils.WorkDayCount(holdRecord.buyDate, d);
            if (holdRecord.expect < 0)
            {
                if(holdDays >= holdRecord.expect * -1)
                {
                    TradeInfo tradeInfo = new TradeInfo()
                    {
                        Direction = TradeDirection.Sell,
                        Code = holdRecord.code,
                        Amount = holdRecord.amount,
                        EntrustPrice = klineItemDay.CLOSE,
                        EntrustDate = d,
                        TradeDate = d,
                        TradePrice = klineItemDay.CLOSE,
                        Stamps = stampduty,
                        Fee = volumecommission,
                        TradeMethod = TradeInfo.TM_AUTO,
                        Reason = "个股预期观望超过"+(-1* holdRecord.expect).ToString()+"天"
                    };
                    return tradeInfo;
                }
            }

            //达到止损线
            if(p_stoploss > 0)
            {
                if(-1* profilt > p_stoploss)
                {
                    TradeInfo tradeInfo = new TradeInfo()
                    {
                        Direction = TradeDirection.Sell,
                        Code = holdRecord.code,
                        Amount = holdRecord.amount,
                        EntrustPrice = klineItemDay.CLOSE,
                        EntrustDate = d,
                        TradeDate = d,
                        TradePrice = klineItemDay.CLOSE,
                        Stamps = stampduty,
                        Fee = volumecommission,
                        TradeMethod = TradeInfo.TM_AUTO,
                        Reason = "到达止损线" + p_stoploss.ToString("F2")
                    };
                    return tradeInfo;
                }
            }

            //达到最大持仓天数
            if(p_maxholddays > 0)
            {
                
                if(holdDays > p_maxholddays)
                {
                    TradeInfo tradeInfo = new TradeInfo()
                    {
                        Direction = TradeDirection.Sell,
                        Code = holdRecord.code,
                        Amount = holdRecord.amount,
                        EntrustPrice = klineItemDay.CLOSE,
                        EntrustDate = d,
                        TradeDate = d,
                        TradePrice = klineItemDay.CLOSE,
                        Stamps = stampduty,
                        Fee = volumecommission,
                        TradeMethod = TradeInfo.TM_AUTO,
                        Reason = "到达最大持仓天数" + p_maxholddays.ToString()
                    };
                    return tradeInfo;
                }
            }
            return null;

        }

        public override bool DoSell(string code, TradeBout bout, DateTime d, Properties strategyParam, BacktestParameter backtestParam, out string reason)
        {
            throw new NotImplementedException();
        }
    }
}
