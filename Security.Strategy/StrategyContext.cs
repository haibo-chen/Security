using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using insp.Utility.Bean;
using insp.Utility.IO;
using insp.Utility.Collections;
using insp.Security.Data;
using insp.Security.Strategy.Evolution;

namespace insp.Security.Strategy
{
    /// <summary>
    /// 策略上下文
    /// </summary>
    public class StrategyContext : Properties,IStrategyContext,IListOperator
    {
        #region 配置管理
        /// <summary>
        /// 日志
        /// </summary>
        private log4net.ILog logger = log4net.LogManager.GetLogger("context");
        /// <summary>
        /// 配置文件
        /// </summary>
        private FileInfo configFile;
        /// <summary>
        /// 配置信息
        /// </summary>
        private StrategyConfiguration configuration = new StrategyConfiguration();
        /// <summary>
        /// 配置信息
        /// </summary>
        public StrategyConfiguration Configuration { get { return configuration; } }
        #endregion

        #region 初始化
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="path"></param>
        public StrategyContext(String path="")
        {
            readConfigfile(path);
        }
        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="path"></param>
        private void readConfigfile(String path = "")
        {
            String configfilename = "strategy.xml";           
            String fullfilename = FileUtils.GetDirectory(path) + configfilename;
            if (!File.Exists(fullfilename))
            {
                throw new Exception("找不到策略文件文件");
            }

            configFile = new FileInfo(fullfilename);
            configuration = FileUtils.XmlFileRead<StrategyConfiguration>(fullfilename, Encoding.UTF8,true);

            //读取买入算法类配置
            if (configuration.buyers == null)
                configuration.buyers = new List<TypeDescriptorElement>();
            foreach(TypeDescriptorElement e in configuration.buyers)
            {
                IBuyer buyer = e.CreateType<IBuyer>();
                if (buyer == null) continue;
                this.buyers.Add(buyer);
            }

            //读出卖出算法配置
            if (configuration.sellers == null)
                configuration.sellers = new List<TypeDescriptorElement>();
            foreach (TypeDescriptorElement e in configuration.sellers)
            {
                ISeller seller = e.CreateType<ISeller>();
                if (seller == null) continue;
                this.sellers.Add(seller);
            }

            //读出回测参数
            backtestParam = new BacktestParameter();
            foreach(Property prop in configuration.backtest.properties)
            {
                backtestParam.Put(prop.Name, prop.Value);
            }

            //读取策略
            foreach(TypePropertyElement e in configuration.strategys)
            {
                if (e.ClassName == null || e.ClassName == "")
                    e.ClassName = "insp.Security.Strategy.StrategyMeta";

                IStrategyMeta meta = e.CreateType<StrategyMeta>();
                if (meta == null) continue;
               

                IBuyer buyer = this.GetBuyer(e.properties.FirstOrDefault(x=>x.Name=="buyer").Value);
                ISeller seller = this.GetSeller(e.properties.FirstOrDefault(x=>x.Name=="seller").Value);
                if (buyer != null)
                    meta.Parameters.AddRange(((Buyer)buyer).PDList);
                if(seller != null)
                    meta.Parameters.AddRange(((Seller)seller).PDList);
                strategys.Add(meta);

                List<Property> ps = meta.Parameters.Check(e.properties);
                Properties props = new Properties(ps);
                strategyParams.Add(props);


            }
        }
        #endregion

        #region 买卖算法管理

        /// <summary>
        /// 所有的买入算法
        /// </summary>
        private List<IBuyer> buyers = new List<IBuyer>();
        /// <summary>
        /// 所有的卖出算法
        /// </summary>
        private List<ISeller> sellers = new List<ISeller>();

        public IBuyer GetBuyer(String name)
        {
            return buyers.FirstOrDefault(x => x.Name == name || x.Caption == name);
        }
        public ISeller GetSeller(String name)
        {
            return sellers.FirstOrDefault(x => x.Name == name || x.Caption == name);
        }
        #endregion

        #region 策略管理
        
        /// <summary>
        /// 策略元信息集合
        /// </summary>
        protected List<IStrategyMeta> strategys = new List<IStrategyMeta>();
        /// <summary>
        /// 策略执行参数
        /// </summary>
        protected List<Properties> strategyParams = new List<Properties>();
        /// <summary>
        /// 策略元信息集合
        /// </summary>
        public List<IStrategyMeta> Strategys { get { return strategys; } }
        /// <summary>
        /// 策略执行参数集
        /// </summary>
        public List<Properties> StrategyParams { get { return strategyParams; } }
        /// <summary>
        /// 查询策略
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IStrategyMeta GetStrategyMeta(String name)
        {
            return strategys.FirstOrDefault(x => x.Name == name || x.Caption == name);
        }
        public Properties GetStrategyParam(String name)
        {
            IStrategyMeta meta = GetStrategyMeta(name);
            if (meta == null)
                return null;
            int inex = strategys.IndexOf(meta);
            return strategyParams[inex];
        }
        public KeyValuePair<IStrategyMeta,Properties> GetStrateMetaAndParam(String name)
        {
            KeyValuePair<IStrategyMeta, Properties> kv = new KeyValuePair<IStrategyMeta, Properties>();
            IStrategyMeta meta = GetStrategyMeta(name);
            Properties props = GetStrategyParam(name);
            if (props == null) props = new Properties();

            String buyerName = props.Get<String>("buyer","");
            Buyer buyer = (Buyer)this.GetBuyer(buyerName);
            if(buyer != null)
            {
                props = buyer.PDC.CreateProperties(props);
            }

            String sellerName = props.Get<String>("seller", "");
            Seller seller = (Seller)this.GetSeller(sellerName);
            if (seller != null)
            {
                props = seller.PDC.CreateProperties(props);
            }

            return new KeyValuePair<IStrategyMeta, Properties>(meta, props);
        }
        #endregion

        #region 回测管理
        /// <summary>
        /// 回测参数
        /// </summary>
        protected BacktestParameter backtestParam = new BacktestParameter();
        /// <summary>
        /// 回测参数
        /// </summary>
        public BacktestParameter BacktestParam { get { return backtestParam; } }

        public void DoTest(String strategyName = "")
        {
            IndicatorRepository repository = new IndicatorRepository(backtestParam.Datapath);
            repository.Initilization();
            backtestParam.Put("repository", repository);


            if (strategyName == null || strategyName == "")
                strategyName = backtestParam.Get<String>("strategy");
            KeyValuePair<IStrategyMeta, Properties> kv = this.GetStrateMetaAndParam(strategyName);
            if (kv.Key == null)
                return;

            IStrategyMeta meta = kv.Key;
            Properties props = kv.Value;
            String version = props.Get<String>("version","");
            IStrategyInstance instance = meta.CreateInstance("1",props,version);
            if (instance == null)
                return;

            if(backtestParam.Optimization != null && backtestParam.Optimization.ToLower().Contains("ga"))
            {
                new EvolutionComputer(backtestParam.Optimization).Start(this);
                return;
            }

            List<Properties> instanceParamSet = spiltStrategyParams(props);

            int taskCount = this.backtestParam.TaskCount;
            List<Task> tasks = new List<Task>();
            List<Executor> executors = new List<Executor>();
            int instanceCountPerTask = instanceParamSet.Count / taskCount;

            List<ExecuteParam> execParams = new List<ExecuteParam>();

            int batchno = backtestParam.BatchNo;
            String resultPath = backtestParam.Resultpath;
            for (int i = 0; i < instanceParamSet.Count; i++)
            {
                int backtestxh = batchno + i + 1;//回测序号
                                                 //结果文件已经有了，跳过
                                                 /*String resultfilename = resultPath + backtestxh + ".result";
                                                 if (System.IO.File.Exists(resultfilename))
                                                     continue;
                                                 */

                Properties instanceProp = instanceParamSet[i];
                Properties backtestProp = backtestParam.Clone();
                backtestProp["serialno"] = backtestxh.ToString();
                backtestProp["batchno"] = batchno.ToString();

                execParams.Add(new ExecuteParam(backtestxh.ToString(), instanceProp, backtestProp));
                if (execParams.Count >= instanceCountPerTask)
                {
                    Executor executor = new Executor(this,meta, "", execParams);
                    executors.Add(executor);
                    execParams.Clear();
                }
            }
            if (execParams.Count > 0)
            {
                Executor executor = new Executor(this,meta, "", execParams);
                executors.Add(executor);
                execParams.Clear();
            }


            System.IO.File.WriteAllText(resultPath + batchno + ".result", meta.GetBatchResultTitle() + System.Environment.NewLine);

            logger.Info("准备执行任务．．．，(任务数=" + executors.Count.ToString() + ")");
            executors.ForEach(x => tasks.Add(x.Go()));
            Task.WaitAll(tasks.ToArray());
        }

        private List<Properties> spiltStrategyParams(Properties strategyProps)
        {
            
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
            return instancePropSet;
        }
        #endregion
    }
}
