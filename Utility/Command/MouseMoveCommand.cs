using insp.Utility.Command;
using insp.Utility.Text;
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

namespace insp.Utility.Command
{
    /// <summary>
    /// 鼠标移动命令
    /// </summary>
    [Text(Name = "Move", Caption = "移动", Alias = new String[] { "MouseMove","Mouse Move","move","move mouse","movemouse","move to","go","goto","go to","移动到","移动鼠标","移动鼠标到","鼠标移动","鼠标移动到","定位","定位到","鼠标定位","鼠标定位到"})]
    public class MouseMoveCommand : CommonCommand
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
            return "移动到" + x.ToString() + "," + y.ToString();
        }

        public override void Execute(CommandContext context)
        {
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(x, y);
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
                MouseMoveCommand command = new MouseMoveCommand();
                String cmdName = command.findNames(cmd);
                if (cmdName == null || cmdName == "")
                    return null;
                String cmdParam = cmd.Substring(cmdName.Length);

                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"[\d{1,}],[\d{1,}]");
                if (!regex.IsMatch(cmdParam))
                {
                    msg = "MouseMoveCommand:" + cmdParam;
                    return null;
                }
                System.Text.RegularExpressions.Match match = regex.Match(cmdParam);
                if (match.Groups.Count < 2)
                {
                    msg = "无法解析命令MouseMoveCommand的参数,匹配不足:" + cmdParam;
                    return null;
                }
                String xString = match.Groups[0].Value;
                String yString = match.Groups[1].Value;


                int x = 0;
                if (!int.TryParse(xString, out x))
                {
                    msg = "无法解析命令WaitCommand的参数:x坐标错误" + cmd + ":" + x;
                    return null;
                }
                int y = 0;
                if (!int.TryParse(yString, out x))
                {
                    msg = "无法解析命令WaitCommand的参数:y坐标错误" + cmd + ":" + y;
                    return null;
                }

                
                command.x = x;
                command.y = y;

                return command;
            }
        }
    }
}
