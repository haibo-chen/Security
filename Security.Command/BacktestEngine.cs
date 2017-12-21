using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

using insp.Utility.Collections;
using insp.Utility.Collections.Tree;
using insp.Utility.Collections.Time;

using insp.Security.Data;
using insp.Security.Data.kline;
using insp.Security.Data.Indicator;

using insp.Utility.IO;
using insp.Utility.Bean;
using insp.Utility.Common;

using insp.Security.Strategy;
using insp.Security.Strategy.Alpha;

namespace insp.Security.Command
{
    /// <summary>
    /// 回测引擎
    /// </summary>
    public class BacktestEngine
    {
        log4net.ILog logger = log4net.LogManager.GetLogger("main");
        #region 回测参数
        /// <summary>
        /// 初始配置文件
        /// </summary>
        private Properties fileprops;
        /// <summary>
        /// 回测参数配置，从fileprops中提取
        /// </summary>
        private Properties backtestProps;
        /// <summary>
        /// 策略参数配置，从fileprops中提取
        /// </summary>
        private Properties strategyProps;

        /// <summary>
        /// 回测初始编号
        /// </summary>
        private int batchno;
        /// <summary>
        /// 回测策略名
        /// </summary>
        private String backteststrategyName;
        /// <summary>
        /// 结果路径
        /// </summary>
        private String resultPath;
        
        /// <summary>
        /// 回测结果文件名
        /// </summary>
        private String backtestsetresultfilename;
        /// <summary>
        /// 策略
        /// </summary>
        private AlphaStrategy4 alpha;
        /// <summary>
        /// 模版实例
        /// </summary>
        private IStrategyInstance template;
        /// <summary>
        /// 执行任务数
        /// </summary>
        private int taskCount;

        #endregion

        #region 初始化
        /// <summary>
        /// 静态初始化
        /// </summary>
        static BacktestEngine()
        {
            //注册转换器
            ConvertUtils.RegisteConvertor<String, TradeInfo>(ConvertUtils.strToObject<TradeInfo>);
            ConvertUtils.RegisteConvertor<TradeInfo, String>(ConvertUtils.objectToStr);
            ConvertUtils.RegisteConvertor<String, TradeDirection>(ConvertUtils.strtoenum<TradeDirection>);
            ConvertUtils.RegisteConvertor<TradeDirection, String>(ConvertUtils.enumtostr<TradeDirection>);
            ConvertUtils.RegisteConvertor<String, TradeIntent>(ConvertUtils.strtoenum<TradeIntent>);
            ConvertUtils.RegisteConvertor<TradeIntent, String>(ConvertUtils.enumtostr<TradeIntent>);

        }
        /// <summary>
        /// 构造方法
        /// </summary>
        public BacktestEngine()
        {
            
        }
        #endregion

        #region 执行
        public void Execute()
        {
            #region 读取策略参数
            //读取参数文件,将参数属性文件分成策略参数和回测参数
            Properties fileprops = Properties.Load(FileUtils.GetDirectory() + "\\alpha.properties", Encoding.UTF8);            
            batchno = fileprops.Get<int>("backtest.batchno");
            backteststrategyName = fileprops.Get<String>("backtest.strategy");
            Dictionary<String, Properties> propSet = fileprops.Spilt();
            backtestProps = propSet["backtest"];
            strategyProps = propSet[backteststrategyName];
            resultPath = FileUtils.GetDirectory(backtestProps.Get<String>("resultpath"));
            backtestsetresultfilename = resultPath + batchno + ".result";
            taskCount = backtestProps.Get<int>("taskcount");
            if (taskCount <= 0) taskCount = 1;
            #endregion

            
            #region 准备策略参数
            //分解策略参数:tParams中的key是参数名，value是所有的值组合
            List<String> tParamNames = new List<string>();
            List<List<String>> tParamValues = new List<List<string>>();
            List<String> keys = strategyProps.Keys;
            foreach(String tKey in keys)
            {
                List<String> tValues = new List<string>();
                String tvalue = strategyProps[tKey].ToString();
                if (tvalue.Contains(","))
                    tValues.AddRange(tvalue.Split(','));
                else
                    tValues.Add(tvalue);
                int tIndex = tKey.IndexOf(".");
                String key = tKey.Substring(tIndex + 1, tKey.Length - tIndex - 1);
                tParamNames.Add(key);
                tParamValues.Add(tValues);
            }
            //生成策略参数集
            List<String>[] combinators = CollectionUtils.Combination<String>(tParamValues.ToArray());
            List<Properties> instancePropSet = new List<Properties>();
            for(int i=0;i< combinators.Length;i++)
            {
                Properties p = new Properties();
                instancePropSet.Add(p);
                List<String> combinator = combinators[i];
                for (int j=0;j< combinator.Count;j++)
                {
                    p.Put(tParamNames[j], combinator[j]);
                }
            }
            logger.Info("准备策略参数：共有"+ instancePropSet.Count.ToString()+"个参数组合");
            #endregion

            #region 生成任务
            List<Task> tasks = new List<Task>();
            List<Executor> executors = new List<Executor>();
            int instanceCountPerTask = instancePropSet.Count / taskCount;
            alpha = new AlphaStrategy4();
            List<ExecuteParam> execParams = new List<ExecuteParam>();
            for (int i= 0; i< instancePropSet.Count;i++)
            {
                int backtestxh = batchno + i + 1;//回测序号
                //结果文件已经有了，跳过
                String resultfilename = resultPath + backtestxh + ".result";
                if (System.IO.File.Exists(resultfilename))
                    continue;
                
                
                Properties instanceProp = instancePropSet[i];
                Properties backtestProp = backtestProps.Clone();
                backtestProp["serialno"] = backtestxh.ToString();
                backtestProp["batchno"] = batchno.ToString();
                
                execParams.Add(new ExecuteParam(backtestxh.ToString(), GetStrateParameterValues(instanceProp)));
                if(execParams.Count>= instanceCountPerTask)
                {
                    Executor executor = new Executor(execParams, resultPath, batchno.ToString());
                    executors.Add(executor);
                    execParams.Clear();
                }                
            }
            if(execParams.Count>0)
            {
                Executor executor = new Executor(execParams, resultPath, batchno.ToString());
                executors.Add(executor);                
                execParams.Clear();
            }
            #endregion

            #region 执行任务 
            System.IO.File.WriteAllText(resultPath + batchno + ".result", alpha.GetBatchResultTitle());

            logger.Info("准备执行任务．．．，(任务数=" + executors.Count.ToString() + ")");
            executors.ForEach(x => tasks.Add(x.Go()));
            Task.WaitAll(tasks.ToArray());

            Console.Read();
            #endregion
        }

        private String GetStrateParameterValues(Properties instanceProp)
        {
            List<String> paramnames = alpha.GetParameterNames();
            StringBuilder str = new StringBuilder();
            for(int i=0;i<paramnames.Count;i++)
            {
                if (str.ToString() != "")
                    str.Append(",");
                str.Append(instanceProp.Get<String>(paramnames[i]));
            }
            return str.ToString();
        }
        
        
        #endregion

        

    }
}
