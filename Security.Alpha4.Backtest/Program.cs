using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;
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

namespace insp.Security.Alpha4.Backtest
{
    class Program
    {
        static Strategy4 alpha;
        static ILog logger = LogManager.GetLogger("main");
        static String backtestxh;
        static Properties backtestProps;
        static Properties strategyProps;
         /// <summary>
        /// 收到的是回测序号和回测参数
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            init();
            
            //取得参数
            if (args == null || args.Length <= 1 || args[0] == null || args[1] == null || args[0] == "" || args[1] == "")
            {
                logger.Info("启动失败，参数错误");
                return;
            }
            backtestxh = args[0];
            String paramStr = args[1];

            //生成策略参数
            alpha = new AlphaStrategy4();
            strategyProps = new Properties();
            List<String> paramnames = alpha.GetParameterNames();
            String[] paramValueArray = paramStr.Split(',');
            if(paramnames.Count != paramValueArray.Length)
            {
                logger.Info("启动失败，策略参数无效："+ paramStr);
                return;
            }
            for(int i=0;i< paramnames.Count;i++)
            {
                strategyProps.Put(paramnames[i], paramValueArray[i]);
            }

            //读取回测参数
            Properties fileprops = Properties.Load(FileUtils.GetDirectory() + "\\alpha.properties", Encoding.UTF8);
            Dictionary<String, Properties> propSet = fileprops.Spilt();
            backtestProps = propSet["backtest"];
            backtestProps["serialno"] = backtestxh.ToString();

            //创建策略实例                
            IStrategyInstance instance = alpha.CreateInstance(backtestxh.ToString(), strategyProps);

            //执行策略实例的回测
            instance.Initilization();
            instance.DoTest(new StrategyContext(), backtestProps);
        }

        
        /// <summary>
        /// 静态初始化
        /// </summary>
        static void init()
        {
            //注册转换器
            ConvertUtils.RegisteConvertor<String, TradeInfo>(ConvertUtils.strToObject<TradeInfo>);
            ConvertUtils.RegisteConvertor<TradeInfo, String>(ConvertUtils.objectToStr);
            ConvertUtils.RegisteConvertor<String, TradeDirection>(ConvertUtils.strtoenum<TradeDirection>);
            ConvertUtils.RegisteConvertor<TradeDirection, String>(ConvertUtils.enumtostr<TradeDirection>);
            ConvertUtils.RegisteConvertor<String, TradeIntent>(ConvertUtils.strtoenum<TradeIntent>);
            ConvertUtils.RegisteConvertor<TradeIntent, String>(ConvertUtils.enumtostr<TradeIntent>);

        }
    }
}
