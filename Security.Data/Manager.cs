using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Collections.Tree;
using UIShell.OSGi;

namespace insp.Security.Data
{
    /// <summary>
    /// 数据管理器
    /// </summary>
    public static class Manager
    {     
        /// <summary>
        /// 配置路径 
        /// </summary>
        public static String ConfigurationPath
        {
            get { return System.AppDomain.CurrentDomain.BaseDirectory + "\\config\\"; }
        }
        /// <summary>
        /// 指标分类器
        /// </summary>
        private readonly static Cataory indicatorCataory = new Cataory();
        /// <summary>
        /// 指标分类器
        /// </summary>
        public static Cataory Cataories { get { return indicatorCataory; } }

        /// <summary>
        /// 指标元信息
        /// </summary>
        private readonly static List<IndicatorMeta> indicatorMetas = new List<IndicatorMeta>();

        /// <summary>
        /// 各指标使用频率统计
        /// </summary>
        private readonly static Dictionary<String, double> indicatorFrencuryUsed = new Dictionary<string, double>();
        /// <summary>
        /// 静态初始化
        /// </summary>
        static Manager()
        {
            //读取指标分类器
            createDefaultCataory();
            //加载插件
            BundleRuntime runtime = new BundleRuntime();
            //注册本地指标集
            registeDefaultIndicatorMeta();
            //加载插件中的指标集
            List<IndicatorMeta> indicatorMetas = runtime.GetService<IndicatorMeta>();
            indicatorMetas.AddRange(indicatorMetas);
        }

        private static void createDefaultCataory()
        {
            indicatorCataory.Childs.Add(new Cataory("基本指标"));
            indicatorCataory.Childs.Add(new Cataory("趋势指标", new string[] { "趋向指标" }));
        }
        /// <summary>
        /// 注册缺省指标类型
        /// </summary>
        private static void registeDefaultIndicatorMeta()
        {            
            indicatorMetas.Add(kline.KLine.Meta);
        }
    }
}
