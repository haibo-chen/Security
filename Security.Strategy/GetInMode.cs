using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Text;
using insp.Utility.Collections;

namespace insp.Security.Strategy
{
    /// <summary>
    /// 建仓方式
    /// </summary>
    public class GetInMode
    {
        public const String SEP = ";";

        /// <summary>固定持仓股数</summary>         
        public const int CONSTHOLDNUM = 1;
        /// <summary>平均持仓股数</summary>         
        public const int AVGHOLDNUM = 2;
        /// <summary>固定持仓资金</summary>         
        public const int CONSTHOLDFUND = 3;
        /// <summary>平均持仓资金</summary>         
        public const int AVGHOLDFUND = 4;

        /// <summary>
        /// 建仓方式
        /// </summary>
        public int mode = CONSTHOLDFUND;
        /// <summary>
        /// 建仓方式解释的参数值
        /// 固定持仓股数:value[0]为固定持仓股数
        /// 平均持仓股数:value[0]为平均持仓股数
        /// </summary>
        public double[] values;
        /// <summary>
        /// 缺省值
        /// </summary>
        public double Value
        {
            get { return values == null || values.Length <= 0 ? 0 : values[0]; }
        }
        /// <summary>
        /// 解析字符串 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static GetInMode Parse(String s)
        {
            GetInMode getin = new GetInMode();
            String[] ss = s.Split(SEP.ToArray());
            if (ss == null || ss.Length <= 0)
                return null;
            if (!int.TryParse(ss[0], out getin.mode))
                return null;
            getin.values = ss.ToDoubleArray(1);
            return getin;
        }
        /// <summary>
        /// 字符串 
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return mode + ((values == null || values.Length <= 0) ? "" : SEP + values.ToString<double>("F2",SEP));
        }

    }
}
