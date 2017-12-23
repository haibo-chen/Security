using insp.Utility.Bean;
using insp.Utility.Collections.Time;
using insp.Utility.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Data.Indicator.Macd
{
    public class MACDItem : TimeSeriesItem, IIndicator
    {
        #region 初始化
        public static readonly PropertyDescriptorCollection pdc;

        //public static readonly PropertyDescriptor PD_CODE = new PropertyDescriptor() { Name = "code", Caption = "代码" };
        public static readonly PropertyDescriptor PD_TIME = new PropertyDescriptor() { Name = "time", Caption = "时间", TypeName = "DateTime" };
        public static readonly PropertyDescriptor PD_DIF = new PropertyDescriptor() { Name = "dif", Caption = "DIF", TypeName = "double" };
        public static readonly PropertyDescriptor PD_DEA = new PropertyDescriptor() { Name = "dea", Caption = "DEA", TypeName = "double" };
        public static readonly PropertyDescriptor PD_MACD = new PropertyDescriptor() { Name = "macd", Caption = "MACD", TypeName = "double" };
        public static readonly PropertyDescriptor PD_CROSS = new PropertyDescriptor() { Name = "cross", Caption = "CROSS", TypeName = "double" };
        /// <summary>
        /// 静态初始化
        /// </summary>
        static MACDItem()
        {
            pdc = new PropertyDescriptorCollection(
                new PropertyDescriptor[]
                {
                    PD_TIME,PD_DIF,PD_DEA,PD_MACD,PD_CROSS
                }
            );
            PropertyObjectUtils.RegisteProperties<MACDItem>(pdc);
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
            return (K)(Object)MACD;
        }
        /// <summary>
        /// 设置缺省值
        /// </summary>
        /// <param name="Value"></param>
        public override void SetDefaultValue(Object Value)
        {
            MACD = (double)Value;
        }
        #endregion

        #region 关键词函数
        [Text(Format = "F2")]
        public double DIF { get { return this.GetValue<double>("dif"); }set { this.SetValue<double>("dif", value); } }
        [Text(Format = "F2")]
        public double DEA { get { return this.GetValue<double>("dea"); } set { this.SetValue<double>("dea", value); } }
        [Text(Format = "F2")]
        public double MACD { get { return this.GetValue<double>("macd"); } set { this.SetValue<double>("macd", value); } }
        [Text(Format = "F2")]
        public double CROSS { get { return this.GetValue<double>("cross"); } set { this.SetValue<double>("cross", value); } }

        #endregion

    }
}
