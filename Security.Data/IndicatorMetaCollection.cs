using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Text;
using insp.Utility.Collections;
using insp.Utility.Collections.Time;

using insp.Security.Data.kline;

namespace insp.Security.Data
{
    /// <summary>
    /// 指标元信息集
    /// </summary>
    public class IndicatorMetaCollection : IListOperator
    {
        #region 常量 
        /// <summary>K线</summary>         
        public const String NAME_KLINE = "kline";
        /// <summary>立体买卖-买线</summary>  
        public const String NAME_CUBEBUY = "cube_buy";
        /// <summary>立体买卖-卖线</summary>  
        public const String NAME_CUBESELL = "cube_sell";
        /// <summary>立体买卖-买卖点</summary>  
        public const String NAME_CUBEPT = "cube_pt";
        /// <summary>资金动向</summary>  
        public const String NAME_FUND_TREND = "fund_trend";
        /// <summary>资金金叉</summary>  
        public const String NAME_FUND_CROSS = "fund_cross";

        /// <summary>K线元信息</summary>         
        public static readonly IndicatorMeta META_KLINE = KLine.Meta;
        /// <summary>立体买卖-买线元信息</summary>  
        public static readonly IndicatorMeta META_CUBEBUY = new IndicatorMeta(NAME_CUBEBUY, "cube", typeof(TimeSeries<ITimeSeriesItem<double>>), IndicatorClass.TimeSeries, null);
        /// <summary>立体买卖-卖线元信息</summary>  
        public static readonly IndicatorMeta META_CUBESELL = new IndicatorMeta(NAME_CUBESELL, "cube", typeof(TimeSeries<ITimeSeriesItem<double>>), IndicatorClass.TimeSeries, null);
        /// <summary>立体买卖-买卖点元信息</summary>  
        public static readonly IndicatorMeta META_CUBEPT = new IndicatorMeta(NAME_CUBEPT, "cube", typeof(TimeSeries<ITimeSeriesItem<char>>), IndicatorClass.TimeSeries, null);
        /// <summary>资金动向元信息</summary>
        public static readonly IndicatorMeta META_FUND_TREND = new IndicatorMeta(NAME_FUND_TREND, "fundtrend", typeof(TimeSeries<ITimeSeriesItem<List<double>>>), IndicatorClass.TimeSeries, null);
        /// <summary>资金金叉元信息</summary>
        public static readonly IndicatorMeta META_FUND_CROSS = new IndicatorMeta(NAME_FUND_CROSS, "fundcross", typeof(TimeSeries<ITimeSeriesItem<double>>), IndicatorClass.TimeSeries, null);

        #endregion

        #region 成员和初始化
        /// <summary>
        ///成员
        /// </summary>
        private List<IndicatorMeta> elements = new List<IndicatorMeta>();
        /// <summary>
        /// 构造方法
        /// </summary>
        public IndicatorMetaCollection()
        {
            createDefault();
        }
        #endregion

        #region 注册和查找
        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="meta"></param>
        public void Registe(IndicatorMeta meta)
        {
            if (meta == null) return;

            int index = elements.IndexOf(elements.FirstOrDefault(x => x.NameInfo.HasName(meta.NameInfo.Name)));
            if (index < 0) elements.Add(meta);
            else elements[index] = meta;
        }
        /// <summary>
        /// 创建缺省元
        /// </summary>
        private void createDefault()
        {
            elements.AddRange(new IndicatorMeta[] { META_KLINE, META_CUBEBUY, META_CUBESELL, META_CUBEPT, META_FUND_TREND, META_FUND_CROSS });

        }
        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IndicatorMeta this[String name]
        {
            get
            {
                return elements.FirstOrDefault(x => x.NameInfo.HasName(name));
            }
            set
            {
                Registe(value);
            }
        }
        #endregion
    }
}
