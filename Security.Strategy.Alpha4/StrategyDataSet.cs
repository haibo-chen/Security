using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Security.Strategy;
using insp.Utility.Bean;
using insp.Utility.Collections;


using insp.Utility.Collections.Tree;
using insp.Utility.Collections.Time;

using insp.Security.Data;
using insp.Security.Data.kline;
using insp.Security.Data.Indicator;
using insp.Utility.Common;

using insp.Utility.IO;
using insp.Utility.Log;
using System.Threading;

namespace insp.Security.Strategy.Alpha
{
    /// <summary>
    /// 回测数据集
    /// </summary>
    public class StrategyDataSet : IDisposable
    {
        #region 基本信息
        /// <summary>
        /// 股票代码
        /// </summary>
        public readonly String code;
        /// <summary>
        /// 数据存储路径 
        /// </summary>
        public String datapath;
        /// <summary>
        /// 结果路径
        /// </summary>
        public String resultpath;

        /// <summary>数据时间单位</summary>         
        public const int DAY = 0;
        /// <summary>数据时间单位</summary>     
        public const int WEEK = 1;
        /// <summary>数据时间单位</summary>     
        public const int MONTH = 2;
        /// <summary>数据个数</summary>     
        public const int NUM = 3;
        /// <summary>时间单位简称</summary> 
        public readonly static String[] TimeUnitNames = {"day","week","month" };

        #endregion

        #region 初始化
        /// <summary>
        /// 构造函数
        /// </summary>
        public StrategyDataSet() { code = ""; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="code"></param>
        public StrategyDataSet(String code,String datapath,String resultpath)
        {
            this.code = code;
            this.datapath = datapath;
            this.resultpath = resultpath;
            if (!this.datapath.EndsWith("\\")) this.datapath = this.datapath + "\\";
            if (!this.resultpath.EndsWith("\\")) this.resultpath = this.resultpath + "\\";
        }
        #endregion



        #region K线


        /// <summary>
        /// K线集合
        /// </summary>
        public readonly KLine[] klines = new KLine[NUM];
        /// <summary>
        /// K线
        /// </summary>
        public KLine klineDay { get { return klines[DAY]; } set { klines[DAY] = value; } }

        /// <summary>
        /// 读取日线数据
        /// </summary>
        public void LoadDayKLine()
        {
            String dayFilename = datapath + (code.StartsWith("6") ? "SH" : "SZ") + code + ".csv";
            KLine dayLine = new KLine(code,TimeUnit.day);
            dayLine.Load(dayFilename, false, ",", new String[] { "时间", "开盘", "最高", "最低", "收盘", "成交量", "成交额" });
            klineDay = dayLine;
        }
        #endregion

        #region 买卖线
        /// <summary>
        /// 买卖线
        /// </summary>
        public readonly TradingLine[] tradeLines = new TradingLine[NUM];
        /// <summary>
        /// 日买卖线
        /// </summary>
        public TradingLine tradeLineDay { get { return tradeLines[DAY]; } set { tradeLines[DAY] = value; } }
        /// <summary>
        /// 买卖线文件名
        /// </summary>
        /// <param name="timeUnitIndex"></param>
        /// <returns></returns>
        public static String[] GetTradeLineFileName(String code,int timeUnitIndex)
        {
            return new String[]
            {
                code + "." + TimeUnitNames[timeUnitIndex]+".itstereo.buy",
                code + "." + TimeUnitNames[timeUnitIndex]+".itstereo.sell",
                code + "." + TimeUnitNames[timeUnitIndex]+".itstereo.pt",
            };            
        }

        /// <summary>
        /// 创建或者读取买卖线
        /// 如果内存中没有买卖线，则到文件读，如果文件中没有，则创建并保存文件
        /// </summary>
        public void CreateOrLoadBuySellLine(int timeunitIndex=0)
        {
            if (klines == null || klines.Length <= 0) return;
            for (int i = 0; i < klines.Length; i++)
            {
                if (timeunitIndex != 0 && timeunitIndex != i)
                    continue;
                if (klines[i] == null || klines[i].Count <= 0)
                    continue;
                if (tradeLines[i] != null) continue;//内存中有
                //判断文件中是否有
                String[] filename = GetTradeLineFileName(code, i);
                if (System.IO.File.Exists(datapath + filename[0]) &&
                    System.IO.File.Exists(datapath + filename[1]) &&
                    System.IO.File.Exists(datapath + filename[2]))
                {
                    this.tradeLines[i].buyLine = new TimeSeries<ITimeSeriesItem<double>>(code, (TimeUnit)timeunitIndex);
                    this.tradeLines[i].buyLine.Load(datapath + filename[0]);
                    this.tradeLines[i].sellLine = new TimeSeries<ITimeSeriesItem<double>>(code, (TimeUnit)timeunitIndex);
                    this.tradeLines[i].sellLine.Load(datapath + filename[1]);
                    this.tradeLines[i].buysellPoints = new TimeSeries<ITimeSeriesItem<char>>(code, (TimeUnit)timeunitIndex);
                    this.tradeLines[i].buysellPoints.Load(datapath + filename[2]);
                }
                else
                {
                    CreateBuySellLine(i);
                }

            }
        }

        /// <summary>
        /// 创建买卖线并保存
        /// </summary>
        /// <param name="timeUnitIndex">时间单位下标</param>
        public void CreateBuySellLine(int timeUnitIndex = 0)
        {
            if (klineDay == null || klineDay.Count <= 0)
                LoadDayKLine();
            if (klineDay == null || klineDay.Count <= 0)
                return;

            for (int i = 0; i < NUM; i++)
            {
                if (timeUnitIndex != 0 && timeUnitIndex != i)
                    continue;
                if (this.klines[i] == null || this.klines[i].Count <= 0)
                    continue;
                this.tradeLines[i] = this.klines[i].indicator_trading_stereo1();
                String[] filenames = GetTradeLineFileName(this.code, i);

                tradeLines[i].buyLine.Save(datapath + filenames[0]);
                tradeLines[i].sellLine.Save(datapath + filenames[1]);
                tradeLines[i].buysellPoints.Save(datapath + filenames[2]);
            }

        }
        #endregion

        #region 资金动向
        /// <summary>
        /// 资金动向线
        /// </summary>
        public readonly TimeSeries<ITimeSeriesItem<List<double>>>[] fundLines = new TimeSeries<ITimeSeriesItem<List<double>>>[NUM];

        /// <summary>
        /// 日资金动向线
        /// </summary>
        public TimeSeries<ITimeSeriesItem<List<double>>> fundDay { get { return fundLines[DAY]; } set { fundLines[DAY] = value; } }

        /// <summary>
        /// 资金低位的日期范围
        /// </summary>
        public List<Range<DateTime>> fundLowRanges = new List<Utility.Common.Range<DateTime>>();
        
        #endregion

        #region 交易记录
        /// <summary>
        /// 交易记录
        /// </summary>
        public TradeRecords tradeRecords = new TradeRecords();

        #endregion

        #region 读写
       
        
        /// <summary>
        /// 保存交易记录
        /// </summary>
        /// <param name="backtestserialno"></param>
        public void SaveTradeRecords(String backtestserialno)
        {
            if (tradeRecords == null || tradeRecords.Count <= 0)
                return;
            String filename = resultpath + code + ".backtest." + backtestserialno + ".result";
            tradeRecords.Save(filename);
        }
        /// <summary>
        /// 取得交易记录文件名
        /// </summary>
        /// <param name="backtestserialno"></param>
        /// <returns></returns>
        public String GetTradeRecordsFileName(String backtestserialno)
        {
            return resultpath + code + ".backtest." + backtestserialno + ".result";
        }
        /// <summary>
        /// 装载数据
        /// </summary>
        /// <param name="code"></param>
        /// <param name="path"></param>
        /// <param name="resultonly"></param>
        /// <param name="backtestserialno"></param>
        /// <returns></returns>
        public static StrategyDataSet CreateOrLoad(String code, String datapath,String resultpath, bool resultonly = false, String backtestserialno = "")
        {

            StrategyDataSet ds = new StrategyDataSet(code, datapath, resultpath);

            //读取回测结果
            String backtestresultfile = resultpath + code + ".backtest." + backtestserialno + ".result";
            if (System.IO.File.Exists(backtestresultfile))
                ds.tradeRecords = TradeRecords.Load(backtestresultfile);
            if (resultonly && ds.tradeRecords.Count>0)
                return ds;

            //加载日线
            ds.LoadDayKLine();
            if (ds.klineDay == null || ds.klineDay.Count <= 0)
                return ds;


            ds.tradeLineDay = new TradingLine();
            //加载或计算买线
            String tradeBuyLineDayFilename = datapath + code + ".day.itstereo.buy";
            if (System.IO.File.Exists(tradeBuyLineDayFilename))
            { 
                ds.tradeLineDay.buyLine = new TimeSeries<ITimeSeriesItem<double>>(code,TimeUnit.day);
                ds.tradeLineDay.buyLine.Load(tradeBuyLineDayFilename);
            }
            else
                ds.CreateBuySellLine();

            //加载或计算卖线
            String tradeSellLineDayFilename = datapath + ds.code + ".day.itstereo.sell";
            if (System.IO.File.Exists(tradeSellLineDayFilename))
                ds.tradeLineDay.sellLine = new TimeSeries<ITimeSeriesItem<double>>(code, TimeUnit.day);
            ds.tradeLineDay.sellLine.Load(tradeSellLineDayFilename);
            //加载买卖点
            String buysellPointsFilename = datapath + ds.code + ".day.itstereo.pt";
            if (System.IO.File.Exists(buysellPointsFilename))
                ds.tradeLineDay.buysellPoints = new TimeSeries<ITimeSeriesItem<char>>(code, TimeUnit.day);
            ds.tradeLineDay.buysellPoints.Load(buysellPointsFilename);

            //加载资金动向线
            String fundDayFilename = datapath + ds.code + ".day.ifmovement";
            if (System.IO.File.Exists(fundDayFilename)) {
                ds.fundDay = new TimeSeries<ITimeSeriesItem<List<double>>>(code, TimeUnit.day);
                ds.fundDay.Load(fundDayFilename);
            } else
            {
                ds.fundDay = ds.klineDay.executeIndicator();
                ds.fundDay.Save(fundDayFilename);
            }
            return ds;
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        /// <summary>
        /// 只保留日线，其它不要
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {

            for (int i = 1; i < klines.Length; i++)
            {                
                klines[i] = null;
            }
            for(int i=0;i<tradeLines.Length;i++)
            {                
                tradeLines[i] = null;
            }
            for(int i=0;i<fundLines.Length;i++)
            {                
                fundLines[i] = null;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~StrategyDataSet() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }

        
        #endregion

    }
}
