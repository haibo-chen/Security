using insp.Utility.Bean;
using insp.Utility.IO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace insp.Security.Strategy
{
    /// <summary>
    /// 回测参数
    /// </summary>
    public class BacktestParameter : Properties
    {
        #region 参数
        public String Runparallel { get { return Get<String>("parallel"); } }
        public String batchno { get { return Get<String>("batchno"); } }
        public int BatchNo { get { return Get<int>("batchno"); } }
        public String Serialno { get { return Get<String>("serialno", ""); } }
        public double Initfunds { get { return Get<double>("funds"); } }

        public int TaskCount { get { return Get<int>("taskcount"); } }
        public double Volumecommission { get { return Get<double>("volumecommission"); } }
        public double Stampduty { get { return Get<double>("stampduty"); } }
        public DateTime BeginDate { get { return Get<DateTime>("begindate"); } }
        public DateTime EndDate { get { return Get<DateTime>("enddate"); } }

        public String Datapath { get { return FileUtils.GetDirectory(Get<String>("datapath")); } }
        public String Resultpath { get { return FileUtils.GetDirectory(Get<String>("resultpath"));} }
        public String Codefilename { get { return Get<String>("codefile"); } }

        public String StateFileName { get { return Resultpath + Serialno + ".state"; } }
        public String ResultFileName { get { return Resultpath + Serialno + ".result"; } }
        public String DateRecordFileName { get { return Resultpath + Serialno + ".date"; } }
        public String DateDetailFileName { get { return Resultpath + Serialno + ".date.bout"; } }

        public String BatchResultFileName { get { return Resultpath + batchno + ".result"; } }
        
        #endregion

        #region 初始化
        /// <summary>
        /// 构造方法
        /// </summary>
        public BacktestParameter()
        {
            
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="props"></param>
        public BacktestParameter(Properties props)
        {
            this.Clone(props);        
        }
        #endregion



    }
}
