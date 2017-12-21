using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using insp.Utility.Text;
using insp.Security.Data.Security;

namespace insp.Security.Import.Info
{
    /// <summary>
    /// 深圳交易所网站数据导入
    /// </summary>
    public class SZTrader
    {
        /// <summary>
        /// 导入股票基本信息
        /// </summary>
        /// <param name="csvFilename">从深交所下载的xls文件转为csv</param>
        /// <returns></returns>
        public List<SecurityProperties> LoadStockInfo(String csvFilename,String bankuan)
        {
            List<SecurityProperties> result = new List<SecurityProperties>();
            String[] lines = File.ReadAllLines(csvFilename);
            String[] propNames = { "地区","代码", "名称", "上市日期", "总股本", "流通股本", "全称", "市场", "类别", "板块", "交易所" };
            for (int i = 1; i < lines.Length; i++)
            {
                String[] ss = lines[i].Spilt2(',');
                if (ss == null || ss.Length < 7) continue;
                StringUtils.Trim(ss);

                int t = ss[4].IndexOf("省");
                if (t < 0)
                    t = ss[4].IndexOf("市");
                String province = t < 0 ? "" : ss[4].Substring(0, t);
                String str = province + "," + ss[5] + "," + ss[6] + "," + ss[7] + "," + ss[8] + "," + ss[9] + "," + ss[2] +",A," + "股票" + "," + bankuan + "," + "SZ";
                SecurityProperties sp = SecurityProperties.Parse(str, propNames);
                if (sp != null)
                    result.Add(sp);
            }
            return result;
        }
    }
}
