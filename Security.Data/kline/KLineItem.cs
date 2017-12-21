using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Text;
using insp.Utility.Bean;
using insp.Utility.Collections.Time;

namespace insp.Security.Data.kline
{
    /// <summary>
    /// K线数据
    /// </summary>
    public class KLineItem : TimeSeriesItem, IIndicator
    {
        #region 初始化
        public static readonly PropertyDescriptorCollection pdc;

        public static readonly PropertyDescriptor PD_CODE = new PropertyDescriptor() { Name = "code", Caption = "代码" };
        public static readonly PropertyDescriptor PD_TIME = new PropertyDescriptor() { Name = "time", Caption = "时间", TypeName = "DateTime" };
        public static readonly PropertyDescriptor PD_OPEN = new PropertyDescriptor() { Name = "open", Caption = "开盘", TypeName = "double" };
        public static readonly PropertyDescriptor PD_CLOSE = new PropertyDescriptor() { Name = "close", Caption = "收盘", TypeName = "double" };
        public static readonly PropertyDescriptor PD_HIGH = new PropertyDescriptor() { Name = "high", Caption = "最高", TypeName = "double" };
        public static readonly PropertyDescriptor PD_LOW = new PropertyDescriptor() { Name = "low", Caption = "最低", TypeName = "double" };
        public static readonly PropertyDescriptor PD_PRECLOSE = new PropertyDescriptor() { Name = "preclose", Caption = "昨收", TypeName = "double" };
        public static readonly PropertyDescriptor PD_CHANGE = new PropertyDescriptor() { Name = "change", Caption = "涨跌", TypeName = "double" };
        public static readonly PropertyDescriptor PD_RANGE = new PropertyDescriptor() { Name = "range", Caption = "幅度", TypeName = "double" };
        public static readonly PropertyDescriptor PD_VOLUMN = new PropertyDescriptor() { Name = "volume", Caption = "成交量", TypeName = "double" };
        public static readonly PropertyDescriptor PD_TURNOVER = new PropertyDescriptor() { Name = "turnover", Caption = "成交额", TypeName = "double" };
        /// <summary>
        /// 静态初始化
        /// </summary>
        static KLineItem()
        {
            pdc = new PropertyDescriptorCollection(
                new PropertyDescriptor[]
                {
                    PD_CODE,PD_TIME,PD_OPEN,PD_CLOSE,PD_HIGH,PD_LOW,PD_PRECLOSE,PD_CHANGE,PD_RANGE,PD_VOLUMN,PD_TURNOVER
                }
            );
            PropertyObjectUtils.RegisteProperties<KLineItem>(pdc);
        }


        #endregion


        #region 实现ITimeSerialItem
        /// <summary>
        /// 取得属性描述符
        /// </summary>
        /// <returns></returns>
        public override PropertyDescriptorCollection GetPropertyDescriptorCollection()
        {
            return pdc;
        }
        /// <summary>
        /// 取得缺省值
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <returns></returns>
        public override K GetDefaultValue<K>()
        {
            return (K)(Object)CLOSE;
        }
        /// <summary>
        /// 设置缺省值
        /// </summary>
        /// <param name="Value"></param>
        public override void SetDefaultValue(Object Value)
        {
            
        }
        #endregion

        #region 关键词函数
        [Text(Format ="F2")]
        public double CLOSE { get { return this.GetValue<double>("close"); } }
        [Text(Format = "F2")]
        public double OPEN { get { return this.GetValue<double>("open"); } }
        [Text(Format = "F2")]
        public double HIGH { get { return this.GetValue<double>("high"); } }
        [Text(Format = "F2")]
        public double LOW { get { return this.GetValue<double>("low"); } }
        [Text(Format = "F2")]
        public double Volume { get { return this.GetValue<double>("volume"); } }
        [Text(Format = "F2")]
        public double Turnover { get { return this.GetValue<double>("turnover"); } }


        #endregion



    }
}