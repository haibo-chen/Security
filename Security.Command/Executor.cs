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

namespace insp.Security.Command
{
    public class ExecuteParam
    {
        public readonly String backtestxh;
        public readonly String strategyParameterValues;
       
        public ExecuteParam(String backtestxh, String strategyParameterValues)
        {
            this.backtestxh = backtestxh;
            this.strategyParameterValues = strategyParameterValues;
        }
        public override string ToString()
        {
            return strategyParameterValues;
        }
    }
    public class Executor
    {

        public readonly static Object batchResultFileLocker = new object();

        public readonly List<ExecuteParam> paramQueue = new List<ExecuteParam>();
        public readonly String resultPath;
        public readonly String batchno;
        public Executor(List<ExecuteParam> paramQueue,String resultPath,String batchno)
        {            
            
            this.paramQueue.AddRange(paramQueue);
            this.resultPath = resultPath;
            this.batchno = batchno;
        }
        public Task Go()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < paramQueue.Count; i++)
                {
                    //准备执行参数
                    log4net.ILog logger = log4net.LogManager.GetLogger(paramQueue[i].backtestxh.ToString());
                    String filename = FileUtils.GetDirectory() + "Security.Alpha4.Backtest.exe";
                    String executparam = paramQueue[i].backtestxh + " " + paramQueue[i].strategyParameterValues;
                    //启动回测程序并等待结束
                    logger.Info("启动回测：" + paramQueue[i] + "...");
                    Process process = Process.Start(filename, executparam);
                    process.WaitForExit();
                    //休眠２秒，然后合并结果文件
                    Thread.Sleep(2000);
                    String resultFile = resultPath + paramQueue[i].backtestxh + ".result";
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


    }
}
