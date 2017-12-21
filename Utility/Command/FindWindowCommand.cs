using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using insp.Utility.Text;
using insp.Utility.Sys;

namespace insp.Utility.Command
{
    [Text(Name = "Find", Caption = "查找", Alias = new String[] { "寻找" })]
    public class FindWindowCommand :CommonCommand<IntPtr>
    {
        /// <summary>类名</summary>         
        private String windowClassName = "";
        /// <summary>窗口名称</summary>         
        private String windowName = "";
        /// <summary>父窗口</summary>         
        private String parentWindow = "";

        /// <summary>
        /// 窗口昵称
        /// </summary>
        public String WindowCaption
        {
            get
            {
                return parentWindow+"[" + windowClassName + "," + windowName + "]";
            }
                
        }
        /// <summary>
        /// 字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "查找" + WindowCaption;
        }

        /// <summary>
        /// 执行方法 
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(CommandContext context)
        {
            IntPtr handle = (IntPtr)context.GetVariableValue(parentWindow);
            result = Win32.FindWindowEx(handle,IntPtr.Zero,windowClassName, windowName);
        }

        /// <summary>
        /// 命令解析器
        /// </summary>
        internal class Parser : ICommandParser
        {
            /// <summary>
            /// 解析命令
            /// </summary>
            /// <param name="cmd"></param>
            /// <param name="msg"></param>
            /// <returns></returns>
            public ICommand TryParse(string cmd, out string msg)
            {
                msg = "";
                FindWindowCommand command = new FindWindowCommand();
                String cmdName = command.findNames(cmd);
                if (cmdName == null || cmdName == "")
                    return null;
                String cmdParam = cmd.Substring(cmdName.Length);


                if (cmdParam.StartsWith("$"))
                {
                    command.parentWindow = "$" + cmdParam.substring(0, "$", "[");
                }
                command.windowClassName = cmdParam.substring(0, "[", ",").Trim();
                command.windowName = cmdParam.substring(0, ",", "]").Trim();

                if (command.windowClassName == "" && command.windowName == "")
                {
                    msg = "FindWindowCommand的窗口控件类别和名称无效:" + cmdParam;
                    return null;
                }


                return command;
            }
        }
    }
}
