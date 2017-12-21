using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Data.Cache
{
    /// <summary>
    /// 数据块头
    /// </summary>
    public class BlockHeader
    {
        /// <summary>
        /// 数据名称
        /// </summary>
        internal String dataName = "";
        /// <summary>
        /// 数据名称
        /// </summary>
        public String DataName { get { return dataName; } }

        /// <summary>
        /// 数据类型
        /// </summary>
        internal String dataTypeName;
        /// <summary>
        /// 数据类型名
        /// </summary>
        public String DataTypeName { get { return dataTypeName; } set { dataTypeName = value; } }

        /// <summary>
        /// 存储名
        /// </summary>
        internal String storeName;
        /// <summary>
        /// 存储名
        /// </summary>
        public String StoreName { get { return storeName; } }
        /// <summary>
        /// 加载的数据范围
        /// </summary>
        internal KeyValuePair<Object, Object> dataRange = new KeyValuePair<object, object>();
        /// <summary>
        /// 加载的数据范围
        /// </summary>
        public KeyValuePair<Object, Object> DataRange { get { return dataRange; } }
        internal bool dirty;
        /// <summary>
        /// 修改标记
        /// </summary>
        public bool Dirty { get { return dirty; } internal set { dirty = value; } }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        internal DateTime lastModifyTime;
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastModifyTime { get { return lastModifyTime; } }

        /// <summary>
        /// 最后保存时间
        /// </summary>
        internal DateTime lastSavedTime;
        /// <summary>
        /// 最后保存时间
        /// </summary>
        public DateTime LastSavedTime { get { return lastSavedTime; } }

        /// <summary>
        /// 最后数据时间
        /// </summary>
        internal DateTime lastDataTime;
        /// <summary>
        /// 最新数据时间
        /// </summary>
        public DateTime LastDataTime { get { return lastDataTime; } }

        /// <summary>
        /// 最大容量
        /// </summary>
        internal int maxCapacity;
        /// <summary>
        /// 最大容量
        /// </summary>
        public int MaxCapacity { get { return maxCapacity; } }
    }
    /// <summary>
    /// 数据块
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Block
    {
        /// <summary>
        /// 基本信息
        /// </summary>
        private BlockHeader header = new BlockHeader();
        /// <summary>
        /// 基本信息
        /// </summary>
        public BlockHeader Header { get { return header; } }
    }
}
