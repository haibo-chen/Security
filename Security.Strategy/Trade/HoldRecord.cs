using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Security.Strategy;
using insp.Utility.Text;
using insp.Utility.Bean;

namespace insp.Security.Strategy
{
    /// <summary>
    /// 持仓记录
    /// </summary>
    public class HoldRecord
    {        
        /// <summary>
        /// 持仓回合信息
        /// </summary>
        public String code;
        /// <summary>
        /// 买入日期
        /// </summary>
        public DateTime buyDate;
        
        /// <summary>
        /// 持仓数量
        /// </summary>
        public int amount;
        /// <summary>
        /// 买入价格
        /// </summary>
        public double buyPrice;
        /// <summary>
        /// 人为控制预期：负数一般用于当前处于亏损状况，用于决定个股的持仓价值
        ///             -1表示立即卖出，
        ///             其它负数表示从买入以来的观望天数，
        ///             0表示预期无效, 
        ///             >0表示预期收益率
        /// </summary>
        public double expect;
                
        /// <summary>
        /// 参数信息
        /// </summary>
        public insp.Utility.Bean.Properties parameters = new insp.Utility.Bean.Properties();


        /// <summary>
        /// 字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return code + "," + buyDate.ToString("yyyyMMdd")+","+ amount.ToString()+"," +expect.ToString("F2")+","+
                   parameters.ToText();
        }
        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static HoldRecord Parse(String s)
        {
            if (s == null || s.Trim() == "") return null;
            
            String[] ss = s.Split('|');
            if (ss == null || ss.Length <= 0) return null;

            HoldRecord r = new HoldRecord();
            r.code = ss[0]==null?"":ss[0].Trim();
            r.buyDate = DateTime.ParseExact(ss[1], "yyyyMMdd", null);
            r.amount = int.Parse(ss[2]);
            r.expect = double.Parse(ss[3]);
            r.parameters = insp.Utility.Bean.Properties.Parse(ss[1]);
            return r;
        }
    }
}
