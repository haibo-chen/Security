using insp.Utility.Bean;
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
using insp.Utility.Date;
using insp.Utility.IO;

namespace insp.Security.Strategy
{
    public interface ITradeOperator
    {
        String Name { get; set; }
        String Caption { get; set; }
    }
    public interface IBuyer : ITradeOperator
    {

        TradeRecords Execute(String code, Properties strategyParam, BacktestParameter backtestParam, ISeller seller = null);
    }
    public interface ISeller : ITradeOperator
    {
        void Execute(TradeRecords tradeRecord, Properties strategyParam, BacktestParameter backtestParam);
    }

    public abstract class TradeOperator : ITradeOperator
    {
        public static log4net.ILog logger = log4net.LogManager.GetLogger("trade");
        protected String name;
        public String Name { get { return name; } set { name = value; } }

        protected String caption;
        public String Caption { get { return caption; } set { caption = value; } }

        protected PropertyDescriptorCollection pdc = new PropertyDescriptorCollection();
        public PropertyDescriptorCollection PDC { get { return pdc; } }
        public List<PropertyDescriptor> PDList { get { return pdc.ToList(); } set { pdc.Clear(); pdc.AddRange(value); } }

        public override string ToString()
        {
            return Name + "," + Caption;
        }
    }

    public abstract class Buyer : TradeOperator,IBuyer
    {
        public abstract List<TradeInfo> DoBuy(Properties strategyParams, DateTime d,StrategyContext context);
        public abstract TradeRecords Execute(string code, Properties strategyParam, BacktestParameter backtestParam, ISeller seller = null);

        /// <summary>
        /// 根据参数配置加载代码
        /// </summary>
        /// <param name="strategyParams"></param>
        /// <param name="backtestParam"></param>
        /// <returns></returns>
        public List<String> LoadCodes(Properties strategyParams, Properties context)
        {
            String codefilename = "";
            if(strategyParams != null)
                codefilename = strategyParams.Get<String>("codefile","");
            if (codefilename == "" && context != null)
                context.Get<String>("codefile","");
            List<String> codes = new List<string>();
            if(System.IO.File.Exists(FileUtils.GetDirectory()+codefilename))
            {
                System.IO.File.ReadAllLines(FileUtils.GetDirectory() + codefilename)
                .ToList().ForEach(x => codes.Add(x.Split(',')[1]));
            }

            String codeNames = "";
            if(strategyParams != null)
                codeNames = strategyParams.Get<String>("codes");
            if(context != null)
                codeNames += context.Get<String>("codes");

            String[] cs = codeNames.Split(',');
            if (cs == null || cs.Length <= 0)
                return codes;
            foreach(String s in cs)
            {
                if (s != null && s.Trim() != "" && !codes.Contains(s))
                    codes.Add(s);
            }
            return codes;
        }
    }


    public abstract class Seller : TradeOperator,ISeller
    {
        /// <summary>
        /// 卖出
        /// </summary>
        /// <param name="holdRecord"></param>
        /// <param name="d"></param>
        /// <param name="strategyParams"></param>
        /// <returns></returns>
        public abstract TradeInfo DoSell(HoldRecord holdRecord, DateTime d, Properties strategyParams,StrategyContext context);

        /// <summary>
        /// 判断指定的回合是否要在d这天卖出
        /// </summary>
        /// <param name="code"></param>
        /// <param name="bout"></param>        
        /// <param name="d"></param>
        /// <param name="strategyParam"></param>
        /// <param name="backtestParam"></param>
        /// <returns>是否进入择机卖出</returns>
        public abstract bool DoSell(String code, TradeBout bout, DateTime d,Properties strategyParam, BacktestParameter backtestParam,out String reason);

        /// <summary>
        /// 执行卖出操作
        /// </summary>
        /// <param name="tradeRecord"></param>
        /// <param name="strategyParam"></param>
        /// <param name="backtestParam"></param>
        public virtual void Execute(TradeRecords tradeRecord, Properties strategyParam, BacktestParameter backtestParam)
        {
            #region 1初始化行情库
            if (tradeRecord == null)
                return;

            IndicatorRepository repository = (IndicatorRepository)backtestParam.Get<Object>("repository");
            if (repository == null)
                return;

            #endregion

            #region 2 取得策略参数

            int p_maxbuynum = strategyParam.Get<int>("maxbuynum",0);
            double p_maxprofilt = strategyParam.Get<double>("maxprofilt");
            int p_maxholddays = strategyParam.Get<int>("maxholddays");
            double p_stoploss = strategyParam.Get<double>("stoploss");
            int p_choosedays = strategyParam.Get<int>("choosedays");
            double p_chooseprofilt = strategyParam.Get<double>("chooseprofilt");
            double p_addholdprofilt = strategyParam.Get<double>("addholdprofilt");
            double p_addholdamount = strategyParam.Get<double>("addholdamount");
            #endregion


            String code = tradeRecord.Code;
            List<TradeBout> bouts = tradeRecord.Bouts;

            #region 3 遍历每一个买入回合
            for (int i = 0; i < bouts.Count; i++)
            {
                #region 3.1 取得该回合的行情数据
                TradeBout bout = bouts[i];
                TimeSerialsDataSet ds = repository[bout.Code];
                if (ds == null) continue;

                if (bout.Completed) continue;//跳过已完成的

                KLine kline = ds.DayKLine;
                if (kline == null) continue;

                bool chooseToSell = false;//择机卖出状态，是指持仓价值较低

                int bIndex = kline.IndexOf(bout.BuyInfo.TradeDate);
                if (bIndex < 0) continue;
                KLineItem klineItemDay = kline[bIndex];
                DateTime d = klineItemDay.Date;


                #endregion

                #region 3.2 如果超过了最大持仓数限制，该回合跳过
                if (p_maxbuynum > 0)
                {
                    //计算当前回合的买入日期这天有多少持仓
                    int count = 0;
                    bouts.ForEach(x => { if (x.Completed && bout.BuyInfo.TradeDate.Date >= x.BuyInfo.TradeDate.Date && bout.BuyInfo.TradeDate.Date < x.SellInfo.TradeDate.Date) count++; });

                    if (count > p_maxbuynum)
                        continue;

                }
                #endregion


                #region 3.3 寻找卖点
                String reason = "";
                for (int index = bIndex + 1; index < kline.Count; index++)
                {
                    klineItemDay = kline[index];
                    d = klineItemDay.Date;


                    #region A 计算以当日最高价和收盘价卖出的盈利
                    double diff = klineItemDay.HIGH - bout.BuyInfo.TradePrice;
                    double percentHigh = diff / bout.BuyInfo.TradePrice;
                    diff = klineItemDay.CLOSE - bout.BuyInfo.TradePrice;
                    double percentClose = diff / bout.BuyInfo.TradePrice;
                    #endregion

                    #region B 盈利超过预定
                    if (p_maxprofilt > 0 && percentHigh >= p_maxprofilt) //盈利超过预定
                    {
                        double price = bout.BuyInfo.TradePrice * (1 + p_maxprofilt);
                        int amount = bout.BuyInfo.Amount;
                        bout.RecordTrade(2, klineItemDay.Date, TradeDirection.Sell, price, amount, backtestParam.Volumecommission, backtestParam.Stampduty, "盈利>=" + p_maxprofilt.ToString("F2"));
                        break;
                    }
                    #endregion

                    #region C 择机卖出状态
                    if (chooseToSell)
                    {
                        for (int t = 0; t < p_choosedays; t++)
                        {
                            index += t;
                            if (index >= kline.Count)
                                break;
                            double percent = (kline[index].HIGH - bout.BuyInfo.TradePrice) / bout.BuyInfo.TradePrice;
                            if (percent >= p_chooseprofilt)
                            {
                                double price = bout.BuyInfo.TradePrice * (1 + p_chooseprofilt);
                                int amount = bout.BuyInfo.Amount;
                                bout.RecordTrade(2, kline[index].Date, TradeDirection.Sell, price, amount, backtestParam.Volumecommission, backtestParam.Stampduty, (reason == "" ? "" : reason + ",并") + "在第" + (t + 1).ToString() + "天择机卖出");
                                break;
                            }
                        }
                        if (!bout.Completed)
                        {
                            if (index >= kline.Count) return;
                            double price = kline[index].CLOSE;
                            int amount = bout.BuyInfo.Amount;
                            bout.RecordTrade(2, kline[index].Date, TradeDirection.Sell, price, amount, backtestParam.Volumecommission, backtestParam.Stampduty, (reason == "" ? "" : reason + ",并") + "择机强制卖出");
                        }
                        break;

                    }
                    #endregion

                    #region D 持仓超过n天进入到择机卖出状态
                    if (p_maxholddays != 0 && CalendarUtils.WorkDayCount(bout.BuyInfo.TradeDate, klineItemDay.Date) >= p_maxholddays)
                    {
                        reason = "持仓超过" + p_maxholddays.ToString() + "天";
                        chooseToSell = true;
                        continue;
                    }
                    #endregion


                    #region E 达到止损线,进入到择机卖出状态
                    double loss = (klineItemDay.LOW - bout.BuyInfo.TradePrice) / bout.BuyInfo.TradePrice;
                    if (p_stoploss > 0 && loss  < 0 && loss < -1 * p_stoploss)
                    {
                        reason = "达到止损" + p_stoploss.ToString("F2");
                        bout.RecordTrade(2, d, TradeDirection.Sell, bout.BuyInfo.TradePrice * (1 - p_stoploss), bout.BuyInfo.Amount, backtestParam.Volumecommission, backtestParam.Stampduty, reason);
                        break;
                    }
                    #endregion

                    #region F调用子类的算法来寻找卖点
                    if (!bout.Completed)
                        chooseToSell = DoSell(code, bout, d, strategyParam, backtestParam,out reason);
                    if (bout.Completed)
                        break;
                    #endregion

                    #region 判断是否加仓或者减仓
                    if(p_addholdprofilt > 0 && p_addholdamount > 0)
                    {
                        if(percentClose > p_addholdprofilt)
                        {

                            int addamount = (int)(bout.BuyInfo.Amount * p_addholdamount);
                            TradeInfo tradeInfo = new TradeInfo();
                            tradeInfo.Code = bout.Code;
                            tradeInfo.Amount = addamount;
                            tradeInfo.Direction = TradeDirection.Buy;
                            tradeInfo.Reason = "加仓";
                            tradeInfo.Fee = backtestParam.Volumecommission;
                            tradeInfo.Stamps = backtestParam.Stampduty;
                            tradeInfo.TradeDate = klineItemDay.Date;
                            tradeInfo.TradePrice = klineItemDay.CLOSE;

                            bout.BuyInfo.TradePrice = (bout.BuyInfo.Amount * bout.BuyInfo.TradePrice + addamount * klineItemDay.CLOSE) / (bout.BuyInfo.Amount + addamount);
                            bout.BuyInfo.Amount += addamount;
                        }
                    }
                    #endregion
                }

                #endregion //寻找卖点结束


            }

            #endregion //遍历每一个买入回合结束
        }
    }
}
