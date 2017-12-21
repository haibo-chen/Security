using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using insp.Utility.Text;
using insp.Utility.Reflection;

namespace insp.Utility.Command
{
    /// <summary>
    /// 命令
    /// </summary>
    public abstract class CommonCommand : ICommand
    {
        protected log4net.ILog logger;
        /// <summary>
        /// 构造方法
        /// </summary>
        public CommonCommand()
        {
            logger = log4net.LogManager.GetLogger(this.getName());
        }
        /// <summary>
        /// 执行记录
        /// </summary>
        /// <param name="context"></param>
        public abstract void Execute(CommandContext context);
        
        
        /// <summary>
        /// 取得结果
        /// </summary>
        /// <returns></returns>
        public virtual Object GetResult()
        {
            return null;
        }
        /// <summary>
        /// 是否是变量
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsVarialbe(String s)
        {
            return s != null && s.StartsWith("$");
        }
        public static bool IsExternal(String s)
        {
            return s != null && s.StartsWith("{$");
        }
        /// <summary>
        /// 寻找str是否以命令名称开始，并返回相应的名称
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected String findNames(String str)
        {
            TextAttribute txtAttr = this.GetType().GetCustomAttribute<TextAttribute>();
            if (txtAttr == null)
                return null;
            List<String> names = txtAttr.GetNames();
            names.Sort((x, y) => y.Length - x.Length);

            foreach (String name in names)
            {
                if (!str.ToLower().StartsWith(name.ToLower()))
                    continue;
                return name;
            }
            return "";
        }
    }
    /// <summary>
    /// 命令
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class CommonCommand<T> : CommonCommand
    {
        /// <summary>
        /// 结果
        /// </summary>
        protected T result;
        /// <summary>
        /// 结果
        /// </summary>
        public T Result { get { return result; } }

        /// <summary>
        /// 取得结果
        /// </summary>
        /// <returns></returns>
        public override Object GetResult()
        {
            return result;
        }
    }
}
