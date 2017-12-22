using insp.Security.Data;
using insp.Utility.Collections;
using insp.Utility.Collections.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Strategy.Alpha
{
    /// <summary>
    /// 大盘参数
    /// </summary>
    public class GrailParameter
    {
        /// <summary>
        /// 上证指数
        /// </summary>
        public const int SZZS = 0;
        /// <summary>
        /// 创业板指
        /// </summary>
        public const int CYBZ = 1;
        /// <summary>
        /// 深圳成指
        /// </summary>
        public const int SZCZ = 2;
        private const String SEP = ";";
        /// <summary>
        /// 有效
        /// </summary>
        public bool Enable { get; set; }
        /// <summary>
        /// 代码
        /// </summary>
        private String[] codes = new String[3];
        /// <summary>
        /// 代码对应的买卖点信息
        /// </summary>
        private TimeSeries<ITimeSeriesItem<char>>[] bspts = new TimeSeries<ITimeSeriesItem<char>>[3]; 
        /// <summary>
        /// 上证指数代码
        /// </summary>
        public String SZCode { get { return codes[SZZS]; } set { codes[SZZS] = value; } }
        /// <summary>
        /// 创业板指
        /// </summary>
        public String CYCode { get { return codes[CYBZ]; } set { codes[CYBZ] = value; } }
        /// <summary>
        /// 深圳成指
        /// </summary>
        public String SCCode { get { return codes[SZCZ]; } set { codes[SZCZ] = value; } }
        /// <summary>
        /// 指数代码
        /// </summary>
        public List<String> GrailCodes { get { return CollectionUtils.AsList(SZCode, CYCode, SCCode); } }
        public GrailParameter()
        {
            SZCode = CYCode = SCCode = "";
        }
        public void Init(IndicatorRepository repository)
        {
            if (repository == null) return;
            for(int i=0;i<codes.Length;i++)
            {
                String code = codes[i];
                TimeSerialsDataSet ds = repository[code];
                if (ds == null) return;
                bspts[i] = ds.CubePtCreateOrLoad(TimeUnit.day);
            }
        }
        /// <summary>
        /// 解析字符串 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static GrailParameter Parse(String s)
        {
            GrailParameter p = new GrailParameter();
            if (s == null || s == "")
                return p;

            String[] ss = s.Split(SEP.ToArray());
            if (ss == null || ss.Length <= 0)
                return p;
            if (ss[0] != null && ss[0].Trim() != "" && ss[0].Trim() != "0")
                p.Enable = true;
            if (ss.Length >= 2 && ss[1] != null)
                p.SZCode = ss[1].Trim();
            if (ss.Length >= 3 && ss[2] != null)
                p.CYCode = ss[2].Trim();
            if (ss.Length >= 4 && ss[3] != null)
                p.SCCode = ss[3].Trim();

            return p;
        }
        /// <summary>
        /// 字符串 
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return Enable ? "1" : "0" + SEP + (SZCode == null ? "" : SZCode) + SEP
                                      + (CYCode == null ? "" : CYCode) + SEP
                                      + (SCCode == null ? "" : SCCode);
        }

        
        
        /// <summary>
        /// 判断某个股票对应大盘类型
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private int getDrailType(String code)
        {
            if (code == null) return -1;
            if (code.StartsWith("6")) return SZZS;
            else if (code.StartsWith("3")) return CYBZ;
            else if (code.StartsWith("002")) return SZCZ;
            else return SZZS;
        }
        /// <summary>
        /// 判断指定的代码是否大盘可以买
        /// </summary>
        /// <param name="d"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool CanBuy(DateTime d,String code)
        {
            if (!Enable) return true;//大盘参数无效，总是认为可以买
            int drailType = getDrailType(code);
            if (drailType < 0) return true; //不认识是哪个盘的，让它买
            if (this.bspts[drailType] == null) return true;
            
            KeyValuePair<int,ITimeSeriesItem> kv = this.bspts[drailType].GetNearest(d);
            if (kv.Key < 0) return true; //没有找到前一个买卖点
            ITimeSeriesItem<char> item = (ITimeSeriesItem<char>)kv.Value;
            if (item.Value == 'B') return true;
            else if (item.Value == 'S') return false;
            return true;
        }
        /// <summary>
        /// 判断大盘数据是否要求代码在d日必须卖
        /// </summary>
        /// <param name="d"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool MustSell(DateTime d, String code)
        {
            if (!Enable) return false;
            int drailType = getDrailType(code);
            if (drailType < 0) return false; //不认识是哪个盘的，不是必须卖
            if (this.bspts[drailType] == null) return false;

            
            KeyValuePair<int, ITimeSeriesItem> kv = bspts[drailType].GetNearest(d);
            if (kv.Key < 0) return false; 
            ITimeSeriesItem<char> item = (ITimeSeriesItem<char>)kv.Value;
            if (item.Value == 'B') return false;
            else if (item.Value == 'S') return true;
            return false;
        }
        
    }
}
