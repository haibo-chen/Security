using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using insp.Utility.IO;
using insp.Utility.Collections.Time;
using insp.Security.Data.Security;
using insp.Security.Data.kline;
using insp.Utility.Text;

namespace insp.Security.Data
{
    
    /// <summary>
    /// 行情库
    /// </summary>
    public class IndicatorRepository
    {
        #region 基本信息
        /// <summary>
        /// 缺省数据路径名
        /// </summary>
        private const String DEFAULT_DATAPATH = "repository";
        /// <summary>
        /// 缓存路径
        /// </summary>
        private String dataPath = "";
        /// <summary>
        /// 远程服务地址
        /// </summary>
        private String serverUrl = "";
        
        /// <summary>
        /// 证劵基本信息
        /// </summary>
        private readonly SecurityPropertiesSet securities = new SecurityPropertiesSet();
        /// <summary>
        /// 证劵基本信息
        /// </summary>
        public SecurityPropertiesSet Securities { get { return securities; } }


        #endregion

        #region 指标元信息
        /// <summary>
        /// 元信息
        /// </summary>
        private IndicatorMetaCollection metas = new IndicatorMetaCollection();
        /// <summary>
        /// 注册元信息
        /// </summary>
        /// <param name="meta"></param>
        public void RegisteMeta(IndicatorMeta meta)
        {
            this.metas.Registe(meta);
        }
        
        #endregion


        #region 初始化
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="dataPath"></param>
        /// <param name="serverUrl"></param>
        public IndicatorRepository(String dataPath, String serverUrl="")
        {
            if (!dataPath.EndsWith("\\"))
                dataPath += "\\";
            this.dataPath = dataPath;
            this.serverUrl = serverUrl;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public void Initilization()
        {
            checkPath();
            LoadSecuritiesInfo();
        }
        
        /// <summary>
        /// 检查数据路径
        /// </summary>
        private void checkPath()
        {
            //路径存在
            if (File.Exists(dataPath))
                return;
            if (dataPath == null || dataPath == "")
                dataPath = DEFAULT_DATAPATH;
            if (dataPath.Contains(":\\"))
            {
                Directory.CreateDirectory(dataPath);
                return;
            }
            if(Directory.Exists(FileUtils.GetDirectory()+dataPath))
            {
                dataPath = FileUtils.GetDirectory() + dataPath;
                if (!dataPath.EndsWith("\\")) dataPath += "\\";
                return;
            }
            DirectoryInfo d = new DirectoryInfo(FileUtils.GetDirectory()).Parent;
            if(Directory.Exists(d.FullName+"\\"+dataPath))
            {
                dataPath = d.FullName + "\\" + dataPath;
                if (!dataPath.EndsWith("\\")) dataPath += "\\";
                return;
            }

            throw new Exception("行情数据仓库存储路径错误:"+dataPath);

        }
        /// <summary>
        /// 证劵基本信息文件名
        /// </summary>
        public const String SecuritiesInfoFileName = "securities.csv";
        /// <summary>
        /// 读取证劵基本信息文件
        /// </summary>
        public void LoadSecuritiesInfo()
        {
            securities.Clear();
            String securitiesFileName = dataPath + SecuritiesInfoFileName;
            if (!File.Exists(securitiesFileName))
                return;
            String[] lines = File.ReadAllLines(securitiesFileName);
            if (lines == null || lines.Length <= 0) return;
            List<SecurityProperties> spList = new List<SecurityProperties>();
            String[] propNames = SecurityProperties.PDC.ConvertAll(x => x.Name).ToArray();
            lines.ToList().ForEach(x => spList.Add(SecurityProperties.Parse(x, propNames)));
            securities.Merge(spList);
        }
        /// <summary>
        /// 保存证劵基本信息
        /// </summary>
        public void SaveSecuritiesInfo()
        {
            String securitiesFileName = dataPath + SecuritiesInfoFileName;
            String[] lines = this.securities.Values.ToList().ConvertAll(x => x.ToString()).ToArray();
            File.WriteAllLines(securitiesFileName,lines);
        }
        #endregion


        #region 时序数据
        /// <summary>
        /// 时序数据
        /// </summary>
        private ConcurrentDictionary<String, TimeSerialsDataSet> timeserials = new ConcurrentDictionary<string, TimeSerialsDataSet>();
        /// <summary>
        /// 索引时序数据
        /// </summary>
        /// <param name="code"></param>
        /// <param name="tu"></param>
        /// <returns></returns>
        public TimeSerialsDataSet this[String code]
        {
            get {
                if(timeserials.ContainsKey(code))
                    return timeserials[code];
                if (!securities.ContainsKey(code))
                    return null;
                SecurityProperties sp = securities[code];
                TimeSerialsDataSet ds = TimeSerialsDataSet.Create(sp, dataPath);
                timeserials[code] = ds;
                return ds;
            }            
        }

        #endregion


    }
}
