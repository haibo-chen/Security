using insp.Utility.Bean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Strategy.Alpha
{
    public class AlphaStrategy7 : IStrategyMeta
    {
        #region 实现IStrategyMeta
        /// <summary>
        /// 名称
        /// </summary>
        public String Name { get { return "alpha7"; } }
        /// <summary>
        /// 名称
        /// </summary>
        public string Caption { get { return "Alpha7.0"; } }
        /// <summary>
        /// 版本
        /// </summary>
        public Version Version { get { return new Version(7,0, 0); } }

        /// <summary>
        /// 参数信息
        /// </summary>
        public PropertyDescriptorCollection Parameters
        {
            get
            {
                return new PropertyDescriptorCollection(
                        new PropertyDescriptor(1, "buy_day_mainlow", "主力线低位", null, "double", "10", "", true, false),
                        new PropertyDescriptor(2, "buy_day_corss", "金叉", null, "int", "10", "", true, false),
                        new PropertyDescriptor(3, "sell_maxholddays", "最大持仓天数", null, "int", "0", "", true, false),
                        new PropertyDescriptor(4, "sell_selectdays", "择机卖出最大天数", null, "int", "0", "", true, false),
                        new PropertyDescriptor(5, "sell_stoploss", "止损线", null, "double", "0", "", true, false),
                        new PropertyDescriptor(6, "sell_sellpt", "卖点", null, "String", "0", "", true, false),                        
                        new PropertyDescriptor(9, "getinMode", "建仓单位资金", null, "insp.Security.Strategy.GetInMode", "3;50000.0", "", true, false),
                        new PropertyDescriptor(10, "grail", "大盘指数", null, "insp.Security.Strategy.Alpha4.GrailParameter", "0", "", true, false)

                );
            }
        }

        /// <summary>
        /// 取得批回测结果标题
        /// </summary>
        /// <returns></returns>
        public String GetBatchResultTitle()
        {
            return "回测编号," + this.GetParameterCaptionString() +
                   ",股票数,回合数,胜率,收益率,总资产,持仓天数(平均/最长),回撤率,每天交易次数(平均/最大)";
        }


        /// <summary>
        /// 策略执行模式
        /// </summary>
        public StrategyExecuteMode Mode { get { return StrategyExecuteMode.Both; } }

        /// <summary>
        /// 字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "策略" + Caption + "(" + Version.ToString(3) + ")";
        }
        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public IStrategyInstance CreateInstance(String id, Properties props, String version = "")
        {
            return new AlphaStrategy7Instance(id, props) { Meta = this };
        }

        #endregion
    }
}
