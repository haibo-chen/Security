using insp.Utility.Bean;
using insp.Utility.Collections;
using insp.Utility.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Strategy
{
    public class BacktestContext : IBacktestContext
    {
        #region 属性和初始化
        private log4net.ILog logger = log4net.LogManager.GetLogger("backtest");
        /// <summary>
        /// 所有参数
        /// </summary>
        private Properties fileParamters = new Properties();
        /// <summary>
        /// 分类参数集
        /// </summary>
        private Dictionary<String, Properties> propSet;
        /// <summary>
        /// 测试参数
        /// </summary>
        private Properties testParamters;
        /// <summary>
        /// 策略工厂
        /// </summary>
        private StrategyFactory factory;
        /// <summary>
        /// 初始资金
        /// </summary>
        public double InitFund { get { return testParamters.Get<double>("initfund"); } }
        /// <summary>
        /// 测试开始日期
        /// </summary>
        public DateTime BeginDate { get { return testParamters.Get<DateTime>("begindate"); } }
        /// <summary>
        /// 测试结束日期
        /// </summary>
        public DateTime EndDate { get { return testParamters.Get<DateTime>("enddate"); } }
        /// <summary>
        /// 批号
        /// </summary>
        public int BatchNo { get { return testParamters.Get<int>("batchno"); } }
        /// <summary>
        /// 结果路径
        /// </summary>
        public String Resultpath { get { return FileUtils.GetDirectory(testParamters.Get<String>("resultpath")); } }
        /// <summary>
        /// 代码文件
        /// </summary>
        public String CodeFile { get { return testParamters.Get<String>("codefile"); } }
        /// <summary>
        /// 并行
        /// </summary>
        public bool Parallel { get { return testParamters.Get<bool>("parallel"); } }
        /// <summary>
        /// 交易手续费
        /// </summary>
        public double Volumecommission { get { return testParamters.Get<double>("volumecommission"); } }
        /// <summary>
        /// 印花税
        /// </summary>
        public double Stampduty { get { return testParamters.Get<double>("stampduty"); } }
        /// <summary>
        /// 任务数
        /// </summary>
        public int TaskCount { get { return testParamters.Get<int>("taskcount"); } }
        /// <summary>
        /// 策略名
        /// </summary>
        public String StrategyNames { get { return testParamters.Get<String>("strategy"); } }
        /// <summary>
        /// 文件参数信息
        /// </summary>
        public Properties FileParamters { get { return fileParamters; } }

        /// <summary>
        /// 测试参数信息
        /// </summary>
        public Properties TestParamters { get { return testParamters; } }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="props"></param>
        public BacktestContext(Properties props,StrategyFactory factory)
        {
            this.fileParamters = props;
            this.factory = factory;
            propSet = fileParamters.Spilt();
            testParamters = propSet["backtest"];
        }

        #endregion

        #region 建造策略实例
        /// <summary>
        /// 策略元信息
        /// </summary>
        private List<IStrategyMeta> strategyMetas = new List<IStrategyMeta>();
        /// <summary>
        /// 策略参数集
        /// </summary>
        private List<List<Properties>> strategyParamSet = new List<List<Properties>>();
        /// <summary>
        /// 构建策略参数
        /// </summary>
        public BacktestContext Build()
        {
            #region 准备策略参数
            foreach (KeyValuePair<String, Properties> kv in propSet)
            {
                if (kv.Key == "backtest") continue;

                IStrategyMeta meta = factory[kv.Key];
                if (meta == null) continue;

                Properties strategyProps = kv.Value;
                List<String> keys = strategyProps.Keys;

                List<String> tParamNames = new List<string>();
                List<List<String>> tParamValues = new List<List<string>>();

                foreach (String tKey in keys)
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

                List<String>[] combinators = CollectionUtils.Combination<String>(tParamValues.ToArray());
                List<Properties> instancePropSet = new List<Properties>();
                for (int i = 0; i < combinators.Length; i++)
                {
                    Properties p = new Properties();
                    instancePropSet.Add(p);
                    List<String> combinator = combinators[i];
                    for (int j = 0; j < combinator.Count; j++)
                    {
                        p.Put(tParamNames[j], combinator[j]);
                    }
                }

                strategyMetas.Add(meta);
                strategyParamSet.Add(instancePropSet);

            }

            //生成策略参数集            
            logger.Info("准备策略参数：共有" + strategyMetas.Count.ToString() + "个策略，" + strategyParamSet.Sum(x => x.Count) + "个参数组合");
            #endregion
            return this;

        }
        /// <summary>
        /// 执行
        /// </summary>
        public void Run()
        {
            String strategyName = StrategyNames;
            IStrategyMeta meta = strategyMetas.FirstOrDefault(x => x.HasName(strategyName));
            if (meta == null)
            {
                logger.Warn("找不到策略:" + strategyName);
            }
            List<Properties> instancePropSet = strategyParamSet[strategyMetas.IndexOf(meta)];
            List<Task> tasks = new List<Task>();
            List<Executor> executors = new List<Executor>();
            int instanceCountPerTask = instancePropSet.Count / TaskCount;
            List<ExecuteParam> execParams = new List<ExecuteParam>();

            int batchno = BatchNo;
            String resultPath = Resultpath;
            for (int i = 0; i < instancePropSet.Count; i++)
            {
                int backtestxh = batchno + i + 1;//回测序号
                                                 //结果文件已经有了，跳过
                /*String resultfilename = resultPath + backtestxh + ".result";
                if (System.IO.File.Exists(resultfilename))
                    continue;
                */
                
                Properties instanceProp = instancePropSet[i];
                Properties backtestProp = testParamters.Clone();
                backtestProp["serialno"] = backtestxh.ToString();
                backtestProp["batchno"] = batchno.ToString();

                execParams.Add(new ExecuteParam(backtestxh.ToString(), instanceProp, backtestProp));
                if (execParams.Count >= instanceCountPerTask)
                {
                    Executor executor = new Executor(meta,"",execParams);
                    executors.Add(executor);
                    execParams.Clear();
                }
            }
            if (execParams.Count > 0)
            {                
                Executor executor = new Executor(meta, "", execParams);
                executors.Add(executor);
                execParams.Clear();
            }

            
            System.IO.File.WriteAllText(resultPath + batchno + ".result", meta.GetBatchResultTitle()+System.Environment.NewLine);

            logger.Info("准备执行任务．．．，(任务数=" + executors.Count.ToString() + ")");
            executors.ForEach(x => tasks.Add(x.Go()));
            Task.WaitAll(tasks.ToArray());
 



        }
        #endregion
    }
}
