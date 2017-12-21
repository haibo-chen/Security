using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Reflection;
using insp.Utility.Text;
using insp.Utility.Bean;

namespace insp.Utility.Collections
{
    public interface ITree : IListOperator
    {
    }

    public interface ITree<T> : ITree
    {
        T Data { get; set; }        
        List<T> ChildDatas { get; }
        T ParentData { get; }
        
    }
    public sealed class TreeBase<T> : ITree<T>
    {
        #region 实现ITree<T>
        protected T data;
        public T Data { get { return data; } set { data = value; } }

        protected readonly List<TreeBase<T>> childs = new List<TreeBase<T>>();
        public List<TreeBase<T>> Childs { get { return new List<TreeBase<T>>(childs); } }

        public List<T> ChildDatas { get { return childs.ConvertAll<T>(x => x.Data).ToList(); } }


        protected TreeBase<T> parent;
        public TreeBase<T> Parent { get { return parent; }  }
        public T ParentData { get { return parent == null ? default(T) : parent.Data; } }

        #endregion
        #region 初始化
        /// <summary>
        /// 构造方法 
        /// </summary>
        public TreeBase(){}
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="data"></param>
        public TreeBase(T data)
        {
            this.data = data;
        }

        public override string ToString()
        {
            if (typeof(T).IsPrimitive)
                return "data:" + ConvertUtils.objectToStr(data);
            return "data:{" + data.ToString() + "}";
        }
        #endregion
        #region 子元素处理
        /// <summary>
        /// 子元素索引
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public int ChildIndexOf(T child)
        {
            TreeBase<T> old = childs.FirstOrDefault<TreeBase<T>>(x => x.Data.Equals(child));
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
        public void ChildAdd(T child)
        {
            TreeBase<T> t = new TreeBase<T>(child);
            t.parent = this;
            parent.childs.Add(t);
        }
        /// <summary>
        /// 如果child已经不存在，则添加
        /// </summary>
        /// <param name="child"></param>
        public void ChildPut(T child)
        {
            TreeBase<T> old = childs.FirstOrDefault<TreeBase<T>>(x => x.Data.Equals(child));
            if (old == null)
            {
                old = new TreeBase<T>(child);
                old.parent = this;
                parent.childs.Add(old);
                return;
            }
            old.parent = this;
        }

        
        #endregion

    }

    public static class TreeUtils
    {

        public static String WriteTree(this ITree tree)
        {
            StringBuilder str = new StringBuilder();
            str.Append(tree.ToString());
            IList list = tree.ListGet("childs");
            if (list == null || list.Count <= 0)
                return str.ToString();

            str.Append("childs:[");
            foreach(ITree child in list)
            {
                str.Append("{");
                str.Append(WriteTree(child));
                str.Append("}");
            }
            str.Append("]");
            return str.ToString();
        }

        public static T ReadTree<T,D>(String str) where T : ITree<D>
        {
            if (str == null || str == "") return default(T);

            T t = (T)typeof(T).Assembly.CreateInstance(typeof(T).FullName);

            int index = 0;
            while(index < str.Length)
            {
                //读取数据
                String datastr = str.substring(index, "data", "childs");
                if (datastr == null || datastr == "") return t;
                int t1 = datastr.IndexOf(":");
                String datavalue = datastr.Substring(t1 + 1, datastr.Length - t1 - 1);
                if (datavalue == "") return t;
                datavalue = datavalue.Trim();
                if (datavalue == "") return t;
                D data = ConvertUtils.strToObject<D>(datavalue);
                t.Data = data;

                //读取子元素
                index += "data".Length + datastr.Length + "childs".Length;
                t1 = str.IndexOf("[", index);
                int t2 = str.LastIndexOf("]");
                String t3 = str.Substring(t1 + 1, t2 - t1 - 1);


            }
            return t;
        }
        private static void ReadTree<T, D>(T t,String str) where T : ITree<D>
        {
            if (str == null || str == "") return;
            int index = 0;

          
            //读取数据
            String datastr = str.substring(index, "data", "childs");
            if (datastr == null || datastr == "") return;
            int t1 = datastr.IndexOf(":");
            String datavalue = datastr.Substring(t1 + 1, datastr.Length - t1 - 1);
            if (datavalue == "") return;
            datavalue = datavalue.Trim();
            if (datavalue == "") return;
            D data = ConvertUtils.strToObject<D>(datavalue);
            t.Data = data;

            //读取子元素
            index += "data".Length + datastr.Length + "childs".Length;
            t1 = str.IndexOf("[", index);
            int t2 = str.LastIndexOf("]");
            String t3 = str.Substring(t1 + 1, t2 - t1 - 1);
            if (t3 == null || t3 == "") return;
            t3 = t3.Trim();
            if (t3 == "") return;


        }





    }
}
