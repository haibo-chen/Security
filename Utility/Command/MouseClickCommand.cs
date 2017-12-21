using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using insp.Utility.Text;
using insp.Utility.Sys;

namespace insp.Utility.Command
{
    /// <summary>
    /// 鼠标点击命令
    /// </summary>
    [Text(Name = "Click", Caption = "点击", Alias = new String[] { "MouseClick", "Mouse Click", "click", "左键","点击" })]
    public class MouseClickCommand : CommonCommand
    {
        /// <summary>
        /// 移动坐标X
        /// </summary>
        private int x;
        /// <summary>
        /// 移动坐标y
        /// </summary>
        private int y;
        /// <summary>
        /// 字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "点击" + x.ToString() + "," + y.ToString();
        }
        /// <summary>
        /// 执行方法 
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(CommandContext context)
        {            
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(x, y);

            try
            {                
                Win32.mouse_event((int)(Win32.MOUSEEVENTF_LEFTDOWN | Win32.MOUSEEVENTF_ABSOLUTE), 0, 0, 0, 0);
                Win32.mouse_event((int)(Win32.MOUSEEVENTF_LEFTUP | Win32.MOUSEEVENTF_ABSOLUTE), 0, 0, 0, 0);
            }
            finally
            {
                System.Threading.Thread.Sleep(1000);
            }
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
                MouseClickCommand command = new MouseClickCommand();
                String cmdName = command.findNames(cmd);
                if (cmdName == null || cmdName == "")
                    return null;
                String cmdParam = cmd.Substring(cmdName.Length);
                String xString = cmdParam.substring(0, "[", ",");
                String yString = cmdParam.substring(0, ",", "]");

                
                int x = 0;
                if (!int.TryParse(xString, out x))
                {
                    msg = "无法解析命令MouseClickCommand的参数:x坐标错误" + cmd + ":" + x;
                    return null;
                }
                int y = 0;
                if (!int.TryParse(yString, out y))
                {
                    msg = "无法解析命令MouseClickCommand的参数:y坐标错误" + cmd + ":" + y;
                    return null;
                }

                
                command.x = x;
                command.y = y;

                return command;
            }
        }
    }
}
