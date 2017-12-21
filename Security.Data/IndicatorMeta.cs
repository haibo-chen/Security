using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Text;
using insp.Utility.Collections.Tree;
using insp.Utility.Bean;
using insp.Utility.Collections.Time;

namespace insp.Security.Data
{
    /// <summary>
    /// 指标数据类别
    /// </summary>
    public enum IndicatorClass
    {
        /// <summary>时间序列数据项</summary>         
        TimeSeriesItem,
        /// <summary>时间序列数据</summary>         
        TimeSeries,
    }

    
    /// <summary>
    /// 指标元信息
    /// </summary>
    public class IndicatorMeta
    {
        #region 名称信息
        
        /// <summary>
        /// 名称信息
        /// </summary>
        private NameInfo nameInfo = new NameInfo();        
        /// 名称信息
        /// </summary>
        public NameInfo NameInfo { get { return nameInfo; } set { nameInfo = value; } }
        /// <summary>
        /// 路径
        /// </summary>
        private String path;
        /// <summary>
        /// 路径
        /// </summary>
        public String Path { get { if (path == null || path == "") return nameInfo.Name;return path; } set { this.path = value; } }
        #endregion

        #region 类型信息
        /// <summary>
        /// 类别
        /// </summary>
        private IndicatorClass indicatorClass;
        /// <summary>
        /// 数据类型
        /// </summary>
        private Type indicatorType;

        /// <summary>
        /// 生成器
        /// </summary>
        private Func<Properties, Object> generator;

        /// <summary>
        /// 类别
        /// </summary>
        public IndicatorClass IndicatorClass { get { return indicatorClass; } }
        /// <summary>
        /// 数据类型
        /// </summary>
        public Type IndicatorType { get { return indicatorType; } }

        

        /// <summary>
        /// 生成器
        /// </summary>
        public Func<Properties, Object> Geneartor { get { return generator; }set { this.generator = value; } }

        #endregion


        #region 初始化

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public IndicatorMeta(String name, String path,Type type, IndicatorClass indicatorClass, Func<Properties, IIndicator> generator)
        {
            nameInfo.Name = name;
            this.indicatorType = type;
            this.indicatorClass = indicatorClass;
            this.Path = path;
            this.generator = generator;
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public IndicatorMeta(NameInfo nameInfo, String path, Type type, IndicatorClass indicatorClass, Func<Properties, IIndicator> generator)
        {
            this.nameInfo = nameInfo;
            this.indicatorType = type;
            this.indicatorClass = indicatorClass;
            this.Path = path;
            this.generator = generator;
        }

        #endregion
    }
}
