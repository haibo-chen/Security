using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Text;

namespace insp.Utility.Collections.Tree
{
    /// <summary>
    /// 名称分类器
    /// </summary>
    public class Cataory : ITree<NameInfo>, IListOperator
    {
        #region 实现ITree<T>
        protected NameInfo data;
        public NameInfo Data { get { return data; } set { data = value; } }

        protected readonly List<Cataory> childs = new List<Cataory>();
        public List<Cataory> Childs { get { return new List<Cataory>(childs); } }

        public List<NameInfo> ChildDatas { get { return childs.ConvertAll<NameInfo>(x => x.Data).ToList(); } }


        protected Cataory parent;
        public Cataory Parent { get { return parent; } }
        public NameInfo ParentData { get { return parent == null ? null : parent.Data; } }

        #endregion
        
        #region 子元素处理
        /// <summary>
        /// 子元素索引
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public int ChildIndexOf(NameInfo child)
        {
            Cataory old = childs.FirstOrDefault<Cataory>(x => x.Data.Equals(child));
            if (old == null) return -1;
            return childs.IndexOf(old);
        }
        /// <summary>
        /// 清除所有子元素
        /// </summary>
        public void ChildClear()
        {
            childs.ForEach(x => x.parent = null);
            childs.Clear();
        }
        /// <summary>
        /// 无论如何都添加
        /// </summary>
        /// <param name="child"></param>
        public void ChildAdd(NameInfo child)
        {
            Cataory t = new Cataory(child);
            t.parent = this;
            parent.childs.Add(t);
        }
        /// <summary>
        /// 如果child已经不存在，则添加
        /// </summary>
        /// <param name="child"></param>
        public void ChildPut(NameInfo child)
        {
            Cataory old = childs.FirstOrDefault<Cataory>(x => x.Data.Equals(child));
            if (old == null)
            {
                old = new Cataory(child);
                old.parent = this;
                parent.childs.Add(old);
                return;
            }
            old.parent = this;
        }


        #endregion
        #region 初始化
        /// <summary>
        /// 构造方法
        /// </summary>
        public Cataory()
        {
            data = new NameInfo();
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="data"></param>
        public Cataory(NameInfo data)
        {
            this.data = data;
        }
        /// <summary>
        /// 构造方法 
        /// </summary>
        /// <param name="name"></param>
        public Cataory(String name)
        {
            data = new NameInfo(name);
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="name"></param>
        /// <param name="caption"></param>
        public Cataory(String name,String caption)
        {
            data = new NameInfo();
        }
        public Cataory(String name, String caption,String description)
        {
            data = new NameInfo(name, caption, description);
        }

        public Cataory(String name, String caption, String description,params String[] alias)
        {
            data = new NameInfo(name, caption, description, alias);
        }
        public Cataory(String name, String[] alias)
        {
            data = new NameInfo(name, alias);
        }

        #endregion

        #region 元素查找
        /// <summary>
        /// 索引器,递归找到对应的子元素
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Cataory this[String name]
        {
            get
            {
                return FindChild(this,name);
            }            
        }
        public static Cataory FindChild(Cataory parent,String name)
        {
            if (parent == null) return null;
            foreach(Cataory e in parent.childs)
            {
                if (e.Data.hasName(name))
                    return e;
                Cataory t = FindChild(e, name);
                if (t != null) return t;
            }
            return null;
        }
        #endregion

    }
}
