using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Security.Strategy;
using insp.Utility.Bean;


namespace insp.Security.Strategy.Alpha
{
    /// <summary>
    /// Alpha策略30
    /// </summary>
    public class AlphaStrategy4 : IStrategyMeta
    {
        #region 实现IStrategyMeta
        /// <summary>
        /// 名称
        /// </summary>
        public String Name { get { return "alpha40"; } }
        /// <summary>
        /// 名称
        /// </summary>
        public string Caption { get { return "Alpha4.0"; } }
        /// <summary>
        /// 版本
        /// </summary>
        public Version Version { get { return new Version(4, 0, 0); } }

        /// <summary>
        /// 参数信息
        /// </summary>
        public PropertyDescriptorCollection Parameters
        {
            get
            {
                return new PropertyDescriptorCollection(
                        new PropertyDescriptor(1, "mainforcelow", "主力线低位", null, "int", "10", "", true, false),
                        new PropertyDescriptor(2, "mainforcerough", "主力线模糊值", null, "int", "0", "", true, false),
                        new PropertyDescriptor(3, "maxprofilt", "最大盈利率", null, "double", "0.05", "", true, false),
                        new PropertyDescriptor(4, "maxholddays", "最大持仓天数", null, "int", "60", "", true, false),
                        new PropertyDescriptor(5, "getinMode", "建仓单位资金",null, "insp.Security.Strategy.GetInMode", "3;50000.0","",true,false),
                        new PropertyDescriptor(6, "stoploss", "止损线", null, "double", "0.1", "", true, false),
                        new PropertyDescriptor(7, "buypointdays", "买点附近天数", null, "int", "3", "", true, false),
                        new PropertyDescriptor(8, "maxbuynum", "最大买入次数", null, "int", "0", "", true, false),
                        new PropertyDescriptor(9, "grail", "大盘指数", null, "insp.Security.Strategy.Alpha4.GrailParameter", "0", "", true, false)                        
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
        public IStrategyInstance CreateInstance(String id,Properties props,String version="")
        {
            return new AlphaStrategy401Instance(id, props) { Meta = this };
        }

        #endregion
    }
}
