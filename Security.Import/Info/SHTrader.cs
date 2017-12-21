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
    /// 上证网站数据导入
    /// </summary>
    public class SHTrader
    {
        /// <summary>
        /// 导入A股基本信息        
        /// </summary>
        /// <param name="csvFilename">从上交所下载的xls文件转为csv</param>
        /// <returns></returns>
        public List<SecurityProperties> LoadStockAInfo(String csvFilename)
        {
            List<SecurityProperties> result = new List<SecurityProperties>();
            String[] lines = File.ReadAllLines(csvFilename);
            String[] propNames = {"代码","名称", "上市日期","总股本","流通股本","全称","市场","类别","板块","交易所" };
            for(int i=1;i<lines.Length;i++)
            {
                String[] ss = lines[i].Split(',');
                if (ss == null || ss.Length < 7) continue;
                StringUtils.Trim(ss);

                String str = ss[2]+","+ss[3] + "," + ss[4] + "," + ss[5] + "," + ss[6] + "," + ss[3] + "," + "A" + "," + "股票" + "," + "主板" + "," + "SH";
                SecurityProperties sp = SecurityProperties.Parse(str,propNames);
                if (sp != null)
                    result.Add(sp);
            }
            return result;
        }
    }
}
