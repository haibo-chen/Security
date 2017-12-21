using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using insp.Utility.Reflection;
using insp.Utility.Text;
using insp.Utility.Common;
using insp.Utility.Collections.Time;
using insp.Utility.Command;
using insp.Utility.Sys;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;

namespace insp.Utility.Command
{
    [Text(Name = "Click", Caption = "点击按钮", Alias = new String[] { "点击" })]
    public class ButtonClickCommand : CommonCommand
    {
        /// <summary>
        /// 按钮变量名
        /// </summary>
        private String btnVariableName;

        /// <summary>
        /// 字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "点击" + btnVariableName;
        }
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(CommandContext context)
        {
            IntPtr btnHandle = (IntPtr)context.GetVariableValue(btnVariableName);
           
            Message msg = Message.Create(btnHandle, Sys.Win32.BM_CLICK, new IntPtr(0), new IntPtr(0));
            Sys.Win32.PostMessage(msg.HWnd, msg.Msg, msg.WParam, msg.LParam);
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
                ButtonClickCommand command = new ButtonClickCommand();
                String cmdName = command.findNames(cmd);
                if (cmdName == null || cmdName == "")
                    return null;

                command.btnVariableName = cmd.Substring(cmdName.Length);
                if (command.btnVariableName == null)
                    command.btnVariableName = "";
                command.btnVariableName = command.btnVariableName.Trim();
                if (command.btnVariableName == "")
                    throw new Exception("点击命令缺少按钮变量:"+cmd);
                return command;
            }
        }
    }
}
