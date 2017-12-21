using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;

using insp.Utility.Text;
using insp.Utility.Sys;

namespace insp.Utility.Command
{
    /// <summary>
    /// 关闭应用命令
    /// </summary>
    [Text(Name = "Close", Caption = "关闭", Alias = new String[] { "stop", "停止" })]
    public class CloseCommand : CommonCommand
    {
        /// <summary>
        /// 待关闭的应用句柄或者名称
        /// 如果同名的应用只打开了一个，也可以通过名称关闭
        /// 句柄可以是变量
        /// </summary>
        private String appHandle;

        /// <summary>
        /// 字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "关闭" + appHandle;
        }

        /// <summary>
        /// 执行方法 
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(CommandContext context)
        {
            logger.Debug("执行 " + this.ToString()+"...");
            IntPtr app = (IntPtr)context.GetVariableValue(appHandle);
            if (app == null)
                return;
            Message msg = Message.Create(app, Win32.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            Sys.Win32.SendMessage(msg.HWnd, msg.Msg, msg.WParam, msg.LParam);
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
                CloseCommand command = new CloseCommand();
                String cmdName = command.findNames(cmd);
                if (cmdName == null || cmdName == "")
                    return null;

                command.appHandle = cmd.Substring(cmdName.Length);
                if (command.appHandle == null)
                    command.appHandle = "";
                command.appHandle = command.appHandle.Trim();
                if (command.appHandle == "")
                    throw new Exception("关闭按钮缺少窗口变量:" + cmd);
                return command;
            }
        }
    }
}
