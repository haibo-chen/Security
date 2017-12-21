using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

using insp.Utility.IO;
using insp.Utility.Bean;

namespace insp.Security.Strategy
{
    public class ExecuteParam
    {
        public readonly String backtestxh;
        public readonly Properties instanceParam;
       
        public ExecuteParam(String backtestxh, Properties instanceParam, Properties backtestProp)
        {
            this.backtestxh = backtestxh;
            this.instanceParam = instanceParam;
        }        
    }
    public class Executor
    {

        public readonly static Object batchResultFileLocker = new object();
        private readonly Properties backtestParams;
        private readonly IStrategyMeta meta;
        private readonly String instanceVersion;
        private List<ExecuteParam> param;


        public Executor(Properties backtestParams,IStrategyMeta meta,String instanceVersion,List<ExecuteParam> param)
        {
            this.backtestParams = backtestParams;
            this.meta = meta;
            this.instanceVersion = instanceVersion;
            this.param = new List<ExecuteParam>(param);
            
        }
        public Task Go()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < param.Count; i++)
                {
                    //准备执行参数
                    log4net.ILog logger = log4net.LogManager.GetLogger(param[i].backtestxh.ToString());
                    
                    //启动回测程序并等待结束
                    
                    IStrategyInstance instance = meta.CreateInstance(param[i].backtestxh, param[i].instanceParam, instanceVersion);
                    logger.Info("启动回测：...");
                    instance.Initilization();
                    instance.DoTest(StrategyContext.Default, backtestParams);

                    String batchno = backtestParams.Get<String>("batchno");
                    String resultPath = backtestParams.Get<String>("resultpath");
                    String resultFile = resultPath + param[i].backtestxh + ".result";
                    String batchresultfile = resultPath + batchno + ".result";
                    if (System.IO.File.Exists(resultFile))
                    {
                        String[] content = System.IO.File.ReadAllLines(resultFile);
                        lock (batchResultFileLocker)
                        {
                            System.IO.File.AppendAllLines(batchresultfile, content);
                        }
                        logger.Info("合并回测结果：" + ((content == null || content.Length <= 0) ? "" : content[0]));
                    }
                    else
                        logger.Warn("没有找到执行结果");
                }
            });
            return task;
            
        }

        private String GetStrateParameterValues(IStrategyMeta meta, Properties instanceProp)
        {
            List<String> paramnames = meta.GetParameterNames();
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < paramnames.Count; i++)
            {
                if (str.ToString() != "")
                    str.Append(",");
                str.Append(instanceProp.Get<String>(paramnames[i]));
            }
            return str.ToString();
        }
    }
}
