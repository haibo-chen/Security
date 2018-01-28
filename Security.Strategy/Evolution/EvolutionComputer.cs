using insp.Security.Data;
using insp.Utility.Bean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Text;

namespace insp.Security.Strategy.Evolution
{
    public class EvolutionComputer
    {
        /// <summary>
        /// 策略上下文 
        /// </summary>
        private StrategyContext context;

        public const int DEFAULT_POPULATION_SIZE = 30;
        /// <summary>
        /// 初始种群数量
        /// </summary>
        protected int initPopulation = DEFAULT_POPULATION_SIZE;

        public const double DEFAULT_CROSSOVER = 0.5;
        protected double pCrossOver = DEFAULT_CROSSOVER;

        public const double DEFAULT_MUTATE = 0.5;
        protected double pMutation = DEFAULT_MUTATE;

        public const int DEFAULT_MAXITERATOR = 1000;

        protected int pMaxIteratorCount = DEFAULT_MAXITERATOR;

        public const double DEFAULT_MAX_PROFILE = 0.5;
        protected double pMaxProfile = DEFAULT_MAX_PROFILE;
        /// <summary>
        /// 日志
        /// </summary>
        protected log4net.ILog logger = log4net.LogManager.GetLogger("GA");

        protected IStrategyMeta meta;

        public EvolutionComputer()
        {

        }
        public EvolutionComputer(String param)
        {
            if (param == null || param == "") return;
            param = param.substring(0, "{", "}");

            String[] ss = param.Split(',');
            if (ss == null || ss.Length < 0)
                return;
            for(int i=0;i<ss.Length;i++)
            {
                if (ss[i] == null || ss[i].Trim() == "")
                    continue;
                String[] s1 = ss[i].Trim().Split('=');
                if (s1 == null || s1.Length < 2 || s1[0].Trim() == "" || s1[1].Trim() == "")
                    continue;
                if(s1[0].Trim() == "种群大小")
                {
                    if (!int.TryParse(s1[1], out initPopulation))
                    {
                        initPopulation = DEFAULT_POPULATION_SIZE;
                    }
                }
                else if(s1[0].Trim() == "迭代次数")
                {
                    if (!int.TryParse(s1[1], out pMaxIteratorCount))
                        pMaxIteratorCount = DEFAULT_MAXITERATOR;
                }
                else if (s1[0].Trim() == "交叉概率")
                {
                    if (!double.TryParse(s1[1], out pCrossOver))
                        pCrossOver = DEFAULT_CROSSOVER;
                }
                else if (s1[0].Trim() == "变异概率")
                {
                    if (!double.TryParse(s1[1], out pMutation))
                        pMutation = DEFAULT_MUTATE;
                }
                else if (s1[0].Trim() == "预期收益")
                {
                    if (!double.TryParse(s1[1], out pMaxProfile))
                        pMaxProfile = DEFAULT_MAX_PROFILE;
                }
            }
           
        }
        public void Start(StrategyContext context)
        {
            this.context = context;

            String strategyName = context.BacktestParam.StrategyName;
            if(strategyName == null || strategyName == "")
            {
                logger.Warn("无效的策略名称，请确保回测参数中定义了策略名称");
                return;
            }

            meta = context.GetStrategyMeta(strategyName);
            if(meta == null)
            {
                logger.Warn("无效的策略名称:"+strategyName);
                return;
            }

            PropertyDescriptorCollection pdc = meta.Parameters;
            if(pdc == null || pdc.Count<=0)
            {
                logger.Warn("策略" + strategyName + "没有定义所需的参数类型");
                return;
            }

            //生成策略参数
            List<Properties> currentParamPopulations = createParamPopulation(pdc,initPopulation);
            int sn = 100;
            currentParamPopulations.ForEach(x => computeFitness(sn++, x));

            int iteratorCount = 1;
            double maxProfilt = 0;
            while (iteratorCount <= pMaxIteratorCount || maxProfilt < pMaxProfile)
            {
                logger.Info("第"+ iteratorCount.ToString()+"代种群生成...");
                //按照适应度排序
                Comparison<Properties> comparsion = (x, y) =>
                {
                    double x1 = x.Get<double>("fitness");
                    double y1 = y.Get<double>("fitness");
                    if (x1 < y1)
                        return 1;
                    else if (x1 > y1)
                        return -1;
                    else
                        return 0;
                };
                currentParamPopulations.Sort(comparsion);
                double[] P = currentParamPopulations.ConvertAll(x => x.Get<double>("fitness")).ToArray();
                maxProfilt = currentParamPopulations[0].Get<double>("fitness");
                logger.Info("本次迭代得到的最大收益：" + maxProfilt.ToString("F3"));


                List<Properties> nextParamPopulations = new List<Properties>(); //下一代
                for (int i = 0; i < currentParamPopulations.Count; i += 2)//逐步生成新一代群体，每次生成两个
                {
                    int select1 = roulette(P);
                    int select2 = select1;
                    while (select1 == select2)
                        select2 = roulette(P);

                    Properties c1 = currentParamPopulations[select1].Clone();
                    Properties c2 = currentParamPopulations[select2].Clone();

                    double t1 = new Random().NextDouble();
                    if (t1 < pCrossOver)
                    {
                        doCross(c1, c2);

                    }
                    t1 = new Random().NextDouble();
                    if (t1 < pMutation)
                    {
                        doMutate(c1);
                        doMutate(c2);
                    }

                    computeFitness(sn++, c1);
                    computeFitness(sn++, c2);

                    nextParamPopulations.Add(c1);
                    nextParamPopulations.Add(c2);
                }

                currentParamPopulations = nextParamPopulations;
                iteratorCount += 1;
            }

            Properties maxParam = currentParamPopulations[0];
            maxProfilt = maxParam.Get<double>("fitness");
            logger.Info("得到的最优收益:"+ maxProfilt.ToString("F3"));
            logger.Info("得到的最优参数组合:");
            logger.Info(pdc.ToString(maxParam));

        }
        /// <summary>
        /// 计算适应度
        /// </summary>
        /// <param name="individual"></param>
        /// <returns></returns>
        private double computeFitness(int serinalno,Properties individual)
        {            
            Properties testParam = context.BacktestParam.Clone();
            testParam["serialno"] = serinalno;
            testParam["batchno"] = serinalno; ;            
            IStrategyInstance instance = meta.CreateInstance(serinalno.ToString(), individual, "");
            logger.Info("启动回测：...");
            instance.Initilization();
            TotalStat stat = instance.DoTest(context, testParam);
            individual.Put("fitness", stat.TotalProfilt);
            return stat.TotalProfilt;
        }

        private void doCross(Properties individual1,Properties individual2)
        {
            PropertyDescriptorCollection pdc = meta.Parameters;

            int count = pdc.Count(x => x.Range != null && x.Range != "");
            int crossPoint = 1;
            if (count == 1)
                crossPoint = 0;
             else
                crossPoint = new Random().Next(0, count);

            int crossIndex = 0;
            for(int i=0;i<pdc.Count;i++)
            {
                if (pdc[i].Range == null || pdc[i].Range == "")
                {
                    continue;
                }

                if (crossIndex == crossPoint)
                {
                    crossIndex = i;
                    break;
                }
                crossIndex += 1;
            }

            for(int i= crossIndex; i<pdc.Count;i++)
            {
                if (pdc[i].Range == null || pdc[i].Range == "")
                    continue;
                String name = pdc[i].Name;
                Object temp = individual1[name];
                individual1[name] = individual2[name];
                individual2[name] = temp;
            }
            
        }

        private void doMutate(Properties individual)
        {
            PropertyDescriptorCollection pdc = meta.Parameters;

            int count = pdc.Count(x => x.Range != null && x.Range != "");
            int crossPoint = 1;
            if (count == 1)
                crossPoint = 0;
            else
                crossPoint = new Random().Next(0, count);

            int crossIndex = 0;
            for (int i = 0; i < pdc.Count; i++)
            {
                if (pdc[i].Range == null || pdc[i].Range == "")
                {                    
                    continue;
                }

                if (crossIndex == crossPoint)
                {
                    crossIndex = i;
                    break;
                }
                crossIndex += 1;
            }


            double value = individual.Get<double>(pdc[crossIndex].Name);
            double min, max;
            pdc[crossIndex].GetRange<double>(out min, out max);
            if (min == max)
                return;
            double r = new Random().NextDouble() * (max - min) + min;
            while (value == r)
                r = new Random().NextDouble() * (max - min) + min;
            if (pdc[crossIndex].Type == typeof(int))
                individual[pdc[crossIndex].Name] = (int)r;
            else
                individual[pdc[crossIndex].Name] = r;
        }
        /// <summary>
        ///  轮盘赌函数     
        /// </summary>
        /// <param name="P">各类对象概率分布</param>
        /// <returns>选择的索引</returns>
        private int roulette(double[] P)
        {
            //如果P中有负值，全部加到正值上
            for (int i = 0; i < P.Length; i++)
                P[i] += -1 * P[P.Length - 1];
            //归一化到0-1之间
            double sum = P.Sum();
            for (int i = 0; i < P.Length; i++)
                P[i] = P[i] / sum;

            double rand = new Random().NextDouble();
            double pointer = 0;//pointer指示每个区段的右边界，从左往右扫描判断
            for (int i = 0; i < P.Length; i++)
            {
                pointer += P[i];
                if (rand <= pointer)
                    return i;
            }
            return -1;
        }

        private List<Properties> createParamPopulation(PropertyDescriptorCollection pdc,int populationSize)
        {
            Properties[] properties = new Properties[populationSize];
            for (int i = 0; i < properties.Length; i++)
            {
                properties[i] = new Properties();
                properties[i].Put("buyer", context.GetStrategyParam(meta.Name).Get<String>("buyer"));
                properties[i].Put("seller", context.GetStrategyParam(meta.Name).Get<String>("seller"));
            }

            foreach (PropertyDescriptor pd in pdc)
            {
                //对于非数值类型的策略参数，直接使用
                if(pd.Type != typeof(int) && pd.Type != typeof(double))
                {
                    for(int i=0;i< properties.Length;i++)
                    {                        
                        properties[i].Put(pd.Name,pd.Default);
                    }
                }
                else if(pd.Type == typeof(int))
                {
                    long tick = DateTime.Now.Ticks;
                    Random rd = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
                    int min = 0, max = 0;
                    bool succ = pd.GetRange<int>(out min, out max);
                    if (!succ)
                        throw new Exception("无法在特定的范围内优化参数：范围应该以-分割:"+pd.Range);
                    
                    
                    for (int i = 0; i < properties.Length; i++)
                    {
                        if (min == max)
                            properties[i].Put(pd.Name, min);
                        else
                        {
                            int md = rd.Next(min, max + 1);
                            properties[i].Put(pd.Name, md);
                        }
                        
                    }
                }
                else if (pd.Type == typeof(double))
                {
                    long tick = DateTime.Now.Ticks;
                    Random rd = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
                    double min = 0, max = 0;
                    bool succ = pd.GetRange<double>(out min, out max);
                    if (!succ)
                        throw new Exception("无法在特定的范围内优化参数：范围应该以-分割:" + pd.Range);


                    for (int i = 0; i < properties.Length; i++)
                    {
                        if (min == max)
                            properties[i].Put(pd.Name, min);
                        else
                        {
                            double md = rd.NextDouble()*(max-min) + min;
                            properties[i].Put(pd.Name, md);
                        }

                    }
                }
            }

            return properties.ToList();
        }

    }
}
