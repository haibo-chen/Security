using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Data.Market
{
    /// <summary>
    /// 市场类型
    /// </summary>
    public enum MarketType
    {
        /// <summary>两者</summary>        
        BOTH,
        /// <summary>证劵</summary>        
        BOND,
        /// <summary>期货</summary>        
        FUTURES,
    }
    /// <summary>
    /// 证劵市场
    /// </summary>
    public class SecurityMarket
    {
        /// <summary>市场类型</summary>   
        public readonly MarketType MarketType;
        /// <summary>编码</summary>   
        public readonly String BM;
        /// <summary>名称</summary>   
        public readonly String Name;

        /// <summary>构造函数</summary>   
        private SecurityMarket(MarketType MarketType, String BM, String Name)
        {
            this.MarketType = MarketType;
            this.BM = BM;
            this.Name = Name;
        }
        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>A股市场</summary>   
        public static readonly SecurityMarket A;
        /// <summary>H股市场</summary>   
        public static readonly SecurityMarket H;
        /// <summary>中金期货市场</summary>   
        public static readonly SecurityMarket ZJQH;
        /// <summary>香港联交所期货市场</summary>   
        public static readonly SecurityMarket LJSQH;

        /// <summary>所有市场数组</summary>   
        private static readonly SecurityMarket[] markets;



        /// <summary>
        /// 查询指定市场
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static SecurityMarket Find(String name)
        {
            if (name == null || name == "")
                return null;
            foreach (SecurityMarket m in markets)
            {
                if (name.Equals(m.BM, StringComparison.CurrentCultureIgnoreCase) ||
                   name.Equals(m.Name, StringComparison.CurrentCultureIgnoreCase))
                    return m;
            }
            return null;
        }

        /// <summary>静态构造函数</summary>   
        static SecurityMarket()
        {
            A = new SecurityMarket(MarketType.BOND, "AEx", "A股");
            H = new SecurityMarket(MarketType.BOND, "HKEx", "H股");
            ZJQH = new SecurityMarket(MarketType.FUTURES, "CTP", "中金期货");
            LJSQH = new SecurityMarket(MarketType.BOTH, "SEHK", "联交所期货");

            markets = new SecurityMarket[] { A, H, ZJQH, LJSQH };
        }
    }
}
