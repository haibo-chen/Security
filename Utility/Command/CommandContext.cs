using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Collections;
using insp.Utility.Text;
using insp.Utility.Reflection;

namespace insp.Utility.Command
{
    /// <summary>
    /// 命令上下文
    /// </summary>
    public class CommandContext
    {
        #region 命令管理
        /// <summary>
        /// 所有命令类型 
        /// </summary>
        private List<ICommandParser> commandParsers = new List<ICommandParser>();
        /// <summary>
        /// 注册命令
        /// </summary>
        /// <param name="cmdType"></param>
        public CommandContext RegisterCommand(ICommandParser parser)
        {
            commandParsers.Add(parser);
            return this;
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="variables"></param>
        /// <param name="parsers"></param>
        public CommandContext(Dictionary<String, Object> external=null, params ICommandParser[] parsers)
        {
            this.external = external;
            if (this.external == null)
                this.external = new Dictionary<string, object>();

            commandParsers.Add(new ButtonClickCommand.Parser());
            commandParsers.Add(new CloseCommand.Parser());
            commandParsers.Add(new FindWindowCommand.Parser());
            commandParsers.Add(new MouseClickCommand.Parser());
            commandParsers.Add(new MouseMoveCommand.Parser());
            commandParsers.Add(new OpenCommand.Parser());
            commandParsers.Add(new TextSendCommand.Parser());
            commandParsers.Add(new WaitCommand.Parser());

            if (parsers == null)
                return;
            foreach (ICommandParser p in parsers)
                commandParsers.Add(p);
        }

        /// <summary>
        /// 寻找接收命令字符串的命令
        /// 如果抛出异常，则表示
        /// </summary>
        /// <param name="cmdStr"></param>
        /// <returns></returns>
        public ICommand Receive(String cmdStr)
        {
            String msg = "";
            foreach(ICommandParser parser in commandParsers)
            {
                ICommand cmd = parser.TryParse(cmdStr,out msg);
                if (cmd != null)
                    return cmd;
                else if (msg != null && msg != "")
                    throw new Exception(msg);
            }
            return null;
        }
        #endregion

        #region 执行过程
        /// <summary>
        /// 变量
        /// </summary>
        private Dictionary<String, Object> variables = new Dictionary<string, object>();
        /// <summary>
        /// 取得变量值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Object GetVariableValue(String name)
        {
            if (!variables.ContainsKey(name))
                return null;
            return variables[name];
        }
        /// <summary>
        /// 设置变量值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void PutVariableValue(String name,Object value)
        {
            variables.SetValue(name, value);
        }
        /// <summary>
        /// 外部变量
        /// </summary>
        private Dictionary<String, Object> external = new Dictionary<string, object>();
        /// <summary>
        /// 外部变量
        /// </summary>
        public Dictionary<String, Object> ExternalVariable { get { return external; } }
        /// <summary>
        /// 设置变量值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void PutExternalValue(String name, Object value)
        {
            external.SetValue(name, value);
        }

        /// <summary>
        /// 取得变量值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Object GetExternalValue(String name)
        {
            if (external.ContainsKey(name))
                return external[name];
            if (!name.Contains("."))
                return null;
            name = name.substring(0, "{$", "}");
            String[] names = name.Split('.');
            if (names == null || names.Length <= 0 || names[0]==null || names[0].Trim()=="")
                return null;
            names[0] = names[0].Trim();
            if (!external.ContainsKey("{$" + names[0] + "}"))
                return null;
            Object value = external["{$" + names[0] + "}"];
            if (value == null) return null;
            return value.FindMemberValue(names, 1);
        }
        /// <summary>
        /// 设置外部变量
        /// </summary>
        /// <param name="external"></param>
        public void SetExternal(Dictionary<String, Object> external)
        {
            this.external = external;
        }
        #endregion

    }
}
