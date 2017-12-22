using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using insp.Utility.IO;
using insp.Utility.Text;
using insp.Utility.Collections.Time;
using insp.Security.Data.Security;
using insp.Security.Data.kline;
using insp.Security.Data.Indicator;
using insp.Utility.Bean;
using insp.Utility.Date;

namespace insp.Security.Data
{
    /// <summary>
    /// 行情数据集
    /// </summary>
    public class TimeSerialsDataSet
    {
        #region 属性
        /// <summary>
        /// 数据路径
        /// </summary>
        private String datapath;
        
        /// <summary>
        /// 代码
        /// </summary>
        private String code;
        /// <summary>
        /// 代码
        /// </summary>
        public String Code { get { return code; } }
        /// <summary>
        /// 时间序列数据基础文件名（交易所缩写+代码）
        /// </summary>
        private String baseName;
        /// <summary>
        /// 元信息集合
        /// </summary>
        private IndicatorMetaCollection metas = new IndicatorMetaCollection();

        /// <summary>
        /// 时序数据
        /// </summary>
        private Dictionary<TimeUnit, ConcurrentDictionary<String, Object>> timeSerials = new Dictionary<TimeUnit, ConcurrentDictionary<string, object>>();
        #endregion

        

        #region 初始化


        /// <summary>
        /// 构造方法
        /// </summary>
        private TimeSerialsDataSet() { }

        /// <summary>
        /// 静态初始化方法
        /// </summary>
        static TimeSerialsDataSet()
        {
            IndicatorMetaCollection.META_CUBEBUY.Geneartor = props =>
            {
                TimeSerialsDataSet tsd = (TimeSerialsDataSet)props["TimeSerialsDataSet"];
                TimeUnit tu = (TimeUnit)props["timeunit"];
                String dataname = (String)props["name"];
                String code = (String)props["code"];

                return tsd.CubeCreate(tu, false).buyLine;
            };
            IndicatorMetaCollection.META_CUBESELL.Geneartor = props =>
            {
                TimeSerialsDataSet tsd = (TimeSerialsDataSet)props["TimeSerialsDataSet"];
                TimeUnit tu = (TimeUnit)props["timeunit"];
                String dataname = (String)props["name"];
                String code = (String)props["code"];

                return tsd.CubeCreate(tu, false).sellLine;
            };
            IndicatorMetaCollection.META_CUBEPT.Geneartor = props =>
            {
                TimeSerialsDataSet tsd = (TimeSerialsDataSet)props["TimeSerialsDataSet"];
                TimeUnit tu = (TimeUnit)props["timeunit"];
                String dataname = (String)props["name"];
                String code = (String)props["code"];

                return tsd.CubeCreate(tu, false).buysellPoints;
            };
            IndicatorMetaCollection.META_FUND_TREND.Geneartor = props =>
            {
                TimeSerialsDataSet tsd = (TimeSerialsDataSet)props["TimeSerialsDataSet"];
                TimeUnit tu = (TimeUnit)props["timeunit"];
                String dataname = (String)props["name"];
                String code = (String)props["code"];

                return tsd.FundTrendCreate(tu);
            };
            IndicatorMetaCollection.META_FUND_CROSS.Geneartor = props =>
            {
                TimeSerialsDataSet tsd = (TimeSerialsDataSet)props["TimeSerialsDataSet"];
                TimeUnit tu = (TimeUnit)props["timeunit"];
                String dataname = (String)props["name"];
                String code = (String)props["code"];

                return tsd.FundTrendCrossCreate(tu);
            };
        }


        /// <summary>
        /// 初始化,缺省初始读取日线数据
        /// </summary>
        /// <param name="datapath"></param>
        public static TimeSerialsDataSet Create(SecurityProperties code,String datapath)
        {
            TimeSerialsDataSet ds = new TimeSerialsDataSet();
            ds.datapath = datapath;
            ds.code = code.Code;
            ds.baseName = code.Exchange + ds.code;
            String dayFilename = ds.baseName + ".csv";

            ds.CreateOrLoad<KLine>("kline", TimeUnit.day);
            /*KLine kline = new KLine(code.Code, TimeUnit.day);
            kline.Load(datapath+"kline\\"+dayFilename, false, ",");
            //kline.Load(dayFilename, false, ",", new String[] { "时间", "开盘", "最高", "最低", "收盘", "成交量", "成交额" });
            ConcurrentDictionary<String, Object> datas = new ConcurrentDictionary<string, object>();
            datas["kline"] = kline;
            ds.timeSerials.Add(TimeUnit.day, datas);*/


            return ds;
        }
        /// <summary>
        /// 数据存储文件名
        /// </summary>
        /// <param name="name"></param>
        /// <param name="timeunit"></param>
        /// <returns></returns>
        private String getFileName(String name,TimeUnit timeunit)
        {            
            return code + "." + name + "." + timeunit.ToString() + ".csv";
        }
        /// <summary>
        /// 全文件名
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="timeunit"></param>
        /// <returns></returns>
        private String GetFullFileName(IndicatorMeta meta, TimeUnit timeunit)
        {
            if (!datapath.EndsWith("\\"))
                datapath += "\\";
            return datapath + meta.Path + "\\" + getFileName(meta.NameInfo.Name,timeunit);
        }
        

        
        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="dataname"></param>
        /// <param name="tu"></param>
        public void Save(String dataname,TimeUnit tu)
        {
            if (dataname == null || dataname == "") return;
            //内存中是否已经存在
            if (!timeSerials.ContainsKey(tu))
                timeSerials.Add(tu, new ConcurrentDictionary<string, object>());
            ConcurrentDictionary<string, object> d = timeSerials[tu];
            if (!d.ContainsKey(dataname))
                return;
            IndicatorMeta meta = metas[dataname];
            if (meta == null)
                throw new Exception("缺少"+dataname+"的元数据");
            ((TimeSeries<ITimeSeriesItem>)d[dataname]).Save(this.GetFullFileName(meta, tu));
            
            
        }
        #endregion

        #region K线
        
        /// <summary>
        /// 日线
        /// </summary>
        public KLine DayKLine
        {
            get { return KLineCreateOrLoad(); }
            set {
                Put((TimeSeries<ITimeSeriesItem>)(Object)value, "kline");
                
            }
        }
        /// <summary>
        /// 周线
        /// </summary>
        public KLine WeekKLine
        {
            get { return KLineCreateOrLoad(TimeUnit.week); }
            set
            {
                this.Put((ITimeSeries)(Object)value, "kline");
                
            }
        }
        /// <summary>
        /// 创建或者加载K线
        /// </summary>
        /// <param name="tu"></param>
        /// <returns></returns>
        public KLine KLineCreateOrLoad(TimeUnit tu = TimeUnit.day)
        {
            return this.CreateOrLoad<KLine>("kline", tu);            
        }
        #endregion

        #region 立体买卖
        /// <summary>
        /// 日买卖线
        /// </summary>
        public TradingLine DayTradeLine
        {
            get { return CubeCreateOrLoad(); }
        }
        /// <summary>
        /// 创建或者加载立体买卖
        /// </summary>
        /// <param name="tu"></param>
        /// <returns></returns>
        public TradingLine CubeCreateOrLoad(TimeUnit tu = TimeUnit.day)
        {
            TradingLine tl = new TradingLine();
            tl.buyLine = CubeBuyCreateOrLoad(tu);
            tl.sellLine = CubeSellCreateOrLoad(tu);
            tl.buysellPoints = CubePtCreateOrLoad(tu);
            return tl;
        }
        /// <summary>
        /// 创建或者加载立体买卖买线
        /// </summary>
        /// <param name="tu"></param>
        /// <returns></returns>
        public TimeSeries<ITimeSeriesItem<double>> CubeBuyCreateOrLoad(TimeUnit tu = TimeUnit.day)
        {
            return this.CreateOrLoad<TimeSeries<ITimeSeriesItem<double>>>(IndicatorMetaCollection.NAME_CUBEBUY,tu);
           

        }
        /// <summary>
        /// 创建或者加载立体买卖卖线
        /// </summary>
        /// <param name="tu"></param>
        /// <returns></returns>
        public TimeSeries<ITimeSeriesItem<double>> CubeSellCreateOrLoad(TimeUnit tu = TimeUnit.day)
        {
            return this.CreateOrLoad<TimeSeries<ITimeSeriesItem<double>>>(IndicatorMetaCollection.NAME_CUBESELL, tu);
        }

        /// <summary>
        /// 创建或者加载立体买卖点
        /// </summary>
        /// <param name="tu"></param>
        /// <returns></returns>
        public TimeSeries<ITimeSeriesItem<char>> CubePtCreateOrLoad(TimeUnit tu = TimeUnit.day)
        {
            return this.CreateOrLoad<TimeSeries<ITimeSeriesItem<char>>>(IndicatorMetaCollection.NAME_CUBEPT, tu);
        }

        /// <summary>
        /// 创建或者读取某个时间单位的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="timeunit"></param>
        /// <returns></returns>
        public T CreateOrLoad<T>(String name, TimeUnit timeunit) where T : ITimeSeries
        {
            T result = default(T);
            //内存中是否已经存在
            if (!timeSerials.ContainsKey(timeunit))
                timeSerials.Add(timeunit, new ConcurrentDictionary<string, object>());
            ConcurrentDictionary<string, object> d = timeSerials[timeunit];
            if (d.ContainsKey(name) && ((IList)d[name]).Count > 0)
                return (T)d[name];
            //查找元信息
            if (name == null || name == "") return default(T);
            IndicatorMeta meta = metas[name];
            if (meta == null)
                throw new Exception("缺少" + name + "的元数据");

            //文件中是否存在，存在则读取
            String fullFilename = GetFullFileName(meta, timeunit);
            if (File.Exists(fullFilename))
            {
                result = (T)typeof(T).GetConstructor(new Type[] { typeof(String), typeof(TimeUnit) }).Invoke(new Object[] { code, timeunit });
                result.Load(fullFilename);
                d[name] = result;
                return result;
            }


            //文件中不存在，尝试用小周期创建大周期
            TimeUnit baseTU = timeunit >= TimeUnit.day ? TimeUnit.day : TimeUnit.minute;
            if (baseTU != timeunit)
            {
                T baseTS = CreateOrLoad<T>(name, baseTU);//取得小周期
                if (baseTS != null)
                {
                    result = (T)baseTS.Zoomout(timeunit);
                    if (result != null)
                    {
                        d[name] = result;
                        result.Save(this.GetFullFileName(meta, timeunit));
                        return result;
                    }
                }
            }

            //尝试使用产生器产生
            if (meta.Geneartor == null)
                return default(T);

            Properties props = new Properties();
            props.Put("TimeSerialsDataSet", this);
            props.Put("timeunit", timeunit);
            props.Put("name", name);
            props.Put("code", code);
            result = (T)meta.Geneartor(props);
            if (result != null)
            {
                this.Put((ITimeSeries)result, name);
            }
            return result;
        }
        /// <summary>
        /// 根据K线生成立体买卖
        /// </summary>
        /// <param name="tu"></param>
        /// <returns></returns>
        public TradingLine CubeCreate(TimeUnit tu = TimeUnit.day,bool forced=true)
        {
            KLine kline = KLineCreateOrLoad(tu);
            TradingLine tradeline = kline.indicator_trading_stereo1();
            tradeline.buyLine.Save(GetFullFileName(IndicatorMetaCollection.META_CUBEBUY, tu));
            tradeline.sellLine.Save(GetFullFileName(IndicatorMetaCollection.META_CUBESELL, tu));
            tradeline.buysellPoints.Save(GetFullFileName(IndicatorMetaCollection.META_CUBEPT, tu));

            if (!timeSerials.ContainsKey(tu))
                timeSerials.Add(tu, new ConcurrentDictionary<string, object>());
            timeSerials[tu][IndicatorMetaCollection.META_CUBEBUY.NameInfo.Name] = tradeline.buyLine;
            timeSerials[tu][IndicatorMetaCollection.META_CUBESELL.NameInfo.Name] = tradeline.sellLine;
            timeSerials[tu][IndicatorMetaCollection.META_CUBEPT.NameInfo.Name] = tradeline.buysellPoints;
            return tradeline;
        }


        #endregion

        #region 资金动向
        /// <summary>
        /// 日资金动向线
        /// </summary>
        public TimeSeries<ITimeSeriesItem<List<double>>> DayFundTrend
        {
            get { return FundTrendCreateOrLoad(); }
            set { this.Put(value, IndicatorMetaCollection.NAME_FUND_TREND); }
        }
        /// <summary>
        /// 周资金动向线
        /// </summary>
        public TimeSeries<ITimeSeriesItem<List<double>>> WeekFundTrend
        {
            get { return FundTrendCreateOrLoad(TimeUnit.week); }
            set { this.Put(value, IndicatorMetaCollection.NAME_FUND_TREND); }
        }
        /// <summary>
        /// 创建或者加载资金动向
        /// </summary>
        /// <param name="tu"></param>
        /// <returns></returns>
        public TimeSeries<ITimeSeriesItem<List<double>>> FundTrendCreateOrLoad(TimeUnit tu = TimeUnit.day)
        {
            return this.CreateOrLoad<TimeSeries<ITimeSeriesItem<List<double>>>>(IndicatorMetaCollection.NAME_FUND_TREND, tu);
        }
        /// <summary>
        /// 资金动向创建
        /// </summary>
        /// <param name="tu"></param>
        /// <returns></returns>
        public TimeSeries<ITimeSeriesItem<List<double>>> FundTrendCreate(TimeUnit tu)
        {
            KLine kline = KLineCreateOrLoad(tu);
            TimeSeries<ITimeSeriesItem<List<double>>> r = kline.executeIndicator();
            r.Save(GetFullFileName(IndicatorMetaCollection.META_FUND_TREND, tu));

            if (!timeSerials.ContainsKey(tu))
                timeSerials.Add(tu, new ConcurrentDictionary<string, object>());
            timeSerials[tu][IndicatorMetaCollection.META_FUND_TREND.NameInfo.Name] = r;
            return r;
        }
        /// <summary>
        /// 日资金交叉线
        /// </summary>
        public TimeSeries<ITimeSeriesItem<double>> DayFundTrendCross
        {
            get { return this.CreateOrLoad<TimeSeries<ITimeSeriesItem<double>>>(IndicatorMetaCollection.NAME_FUND_CROSS,TimeUnit.day); }
            set { this.Put(value, IndicatorMetaCollection.NAME_FUND_CROSS); }
        }
        /// <summary>
        /// 周资金交叉线
        /// </summary>
        public TimeSeries<ITimeSeriesItem<double>> WeekFundTrendCross
        {
            get { return this.CreateOrLoad<TimeSeries<ITimeSeriesItem<double>>>(IndicatorMetaCollection.NAME_FUND_CROSS, TimeUnit.week); }
            set { this.Put(value, IndicatorMetaCollection.NAME_FUND_CROSS); }
        }

        /// <summary>
        /// 创建或者加载资金动向交叉点
        /// </summary>
        /// <param name="tu"></param>
        /// <returns></returns>
        public TimeSeries<ITimeSeriesItem<double>> FundTrendCrossCreateOrLoad(TimeUnit tu = TimeUnit.day)
        {
            return this.CreateOrLoad<TimeSeries<ITimeSeriesItem<double>>>(IndicatorMetaCollection.NAME_FUND_CROSS,tu);
        }
        /// <summary>
        /// 资金动向交叉点创建
        /// </summary>
        /// <param name="tu"></param>
        /// <returns></returns>
        public TimeSeries<ITimeSeriesItem<double>> FundTrendCrossCreate(TimeUnit tu)
        {
            TimeSeries<ITimeSeriesItem<List<double>>> ts = this.CreateOrLoad<TimeSeries<ITimeSeriesItem<List<double>>>>(IndicatorMetaCollection.NAME_FUND_TREND,tu);
            if (ts == null)
                return null;
            TimeSeries<ITimeSeriesItem<double>> corss = new TimeSeries<ITimeSeriesItem<double>>();
            double prevflag = ts[0].Value[0] - ts[0].Value[1];
            for (int i = 1; i < ts.Count; i++)
            {
                double flag = ts[i].Value[0] - ts[i].Value[1];
                if (prevflag < 0 && flag > 0)
                {
                    TimeSeriesItem<double> item = new TimeSeriesItem<double>()
                    {
                        Date = ts[i].Date,
                        Value = flag
                    };
                    corss.Add(item);
                }
                else if (prevflag > 0 && flag < 0)
                {
                    TimeSeriesItem<double> item = new TimeSeriesItem<double>()
                    {
                        Date = ts[i].Date,
                        Value = flag
                    };
                    corss.Add(item);
                }
                prevflag = flag;
            }
            return corss;
        }
        #endregion

        #region 通用方法
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tu"></param>
        /// <returns></returns>
        public ITimeSeries this[String name, TimeUnit tu]
        {
            get
            {
                IndicatorMeta meta = metas[name];
                if (meta == null) return null;

                return (ITimeSeries)this.GetType().GetMethod("CreateOrLoad").MakeGenericMethod(new Type[] { meta.IndicatorType }).Invoke(this, new Object[]{ name, tu});
            }
            set
            {
                this.Put(value, name);
            }
        }
        /// <summary>
        /// 创建时序数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="timeunit"></param>
        /// <param name="forced">true表示如果数据已经有，会删除重建</param>
        /// <returns></returns>
        public ITimeSeries Create(String name, TimeUnit timeunit, bool forced=true)
        {
            //内存中是否已经存在
            if (!timeSerials.ContainsKey(timeunit))
                timeSerials.Add(timeunit, new ConcurrentDictionary<string, object>());
            ConcurrentDictionary<string, object> d = timeSerials[timeunit];
            if (d.ContainsKey(name) && ((IList)d[name]).Count > 0 && !forced)
                return (ITimeSeries)d[name];
            //查找元信息
            if (name == null || name == "") return null;
            IndicatorMeta meta = metas[name];
            if (meta == null)
                throw new Exception("缺少" + name + "的元数据");

            //文件中是否存在，存在则读取
            ITimeSeries result = null;
            String fullFilename = GetFullFileName(meta, timeunit);
            if (File.Exists(fullFilename) && !forced)
            {
                result = (ITimeSeries)meta.IndicatorType.GetConstructor(new Type[] { typeof(String), typeof(TimeUnit) }).Invoke(new Object[] { code, timeunit });
                result.Load(fullFilename);
                d[name] = result;
                return result;
            }


            //文件中不存在，尝试用小周期创建大周期
            TimeUnit baseTU = timeunit >= TimeUnit.day ? TimeUnit.day : TimeUnit.minute;
            if (baseTU != timeunit)
            {
                ITimeSeries baseTS = Create(name, baseTU,false);//取得小周期
                if (baseTS != null)
                {
                    result = baseTS.Zoomout(timeunit);
                    if (result != null)
                    {
                        d[name] = result;
                        result.Save(this.GetFullFileName(meta, timeunit));
                        return result;
                    }
                }
            }

            //尝试使用产生器产生
            if (meta.Geneartor == null)
                return null;

            Properties props = new Properties();
            props.Put("TimeSerialsDataSet", this);
            props.Put("timeunit", timeunit);
            props.Put("name", name);
            props.Put("code", code);
            result = (ITimeSeries)meta.Geneartor(props);
            if (result != null)
            {
                this.Put(result, name);
            }
            return result;
        }
        /// <summary>
        /// 记录数据
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="name"></param>
        public void Put(ITimeSeries ts,String name)
        {
            TimeUnit tu = ts.TimeUnit;
            if (!this.timeSerials.ContainsKey(tu))
                this.timeSerials.Add(tu, new ConcurrentDictionary<string, object>());
            ConcurrentDictionary<string, object> d = this.timeSerials[tu];
            d[name] = ts;

            IndicatorMeta meta = metas[name];
            if (meta == null)
                throw new Exception("缺少"+name+"的元数据");
            String filename = this.GetFullFileName(meta, TimeUnit.week);
            ts.Save(filename);
        }
        #endregion
    }
}
