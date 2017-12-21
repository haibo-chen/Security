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
        /// <summary>
        /// 数据路径
        /// </summary>
        public String datapath, resultpath, codefilename;
        /// <summary>
        /// 初始资金
        /// </summary>
        public double initfunds;

        /// <summary>
        /// 回测开始日期
        /// </summary>
        public DateTime beginDate;
        /// <summary>
        /// 回测结束日期
        /// </summary>
        public DateTime endDate;

        /// <summary>
        /// 是否并行
        /// </summary>
        public String runparallel = "false";
        /// <summary>
        /// 回测批号
        /// </summary>
        public String batchno;
        /// <summary>
        /// 回测序号
        /// </summary>
        public String serialno;
        /// <summary>
        /// 卷商交易佣金
        /// </summary>
        public double volumecommission = 0.0025;
        /// <summary>
        /// 印花税
        /// </summary>
        public double stampduty = 0.001;
    

        /// <summary>
        /// 回测状态文件
        /// </summary>
        public String StateFileName;
        /// <summary>
        /// 按照日期记录的文件名
        /// </summary>
        public String DateRecordFileName;
        /// <summary>
        /// 按照日期的详细记录文件名
        /// </summary>
        public String DateDetailFileName;
        /// <summary>
        /// 结果记录
        /// </summary>
        public String ResultFileName;
        /// <summary>
        /// 批回测结果文件
        /// </summary>
        public String BatchResultFileName;

        /// <summary>
        /// 初始资金
        /// </summary>
        public double InitFund { get { return this.initfunds; } }
        /// <summary>
        /// 测试开始日期
        /// </summary>
        public DateTime BeginDate { get { return beginDate; } }
        /// <summary>
        /// 测试结束日期
        /// </summary>
        public DateTime EndDate { get { return endDate; } }

        
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
            Init();
        }
        /// <summary>
        /// 初始化参数值
        /// </summary>
        public void Init()
        {
            runparallel = Get<String>("parallel");
            batchno = Get<String>("batchno");
            serialno = Get<String>("serialno","");
            initfunds = Get<double>("funds");

            volumecommission = Get<double>("volumecommission");
            stampduty = Get<double>("stampduty");
            beginDate = Get<DateTime>("begindate");
            endDate = Get<DateTime>("enddate");
            
            datapath = FileUtils.GetDirectory(Get<String>("datapath"));
            resultpath = FileUtils.GetDirectory(Get<String>("resultpath"));
            codefilename = Get<String>("codefile");

            StateFileName = resultpath + serialno + ".state";
            ResultFileName = resultpath + serialno + ".result";
            DateRecordFileName = resultpath + serialno + ".date";
            DateDetailFileName = resultpath + serialno + ".date.bout";

            BatchResultFileName = resultpath + batchno + ".result";
        }
        #endregion



    }
}
