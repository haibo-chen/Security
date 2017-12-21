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
    /// <summary>
    /// 打开命令
    /// open 计算器(c:\windows\comput.exe) [max|min] [hide] [x=d] [y=d] [w=d] [h=d]
    /// </summary>
    [Text(Name = "Open", Caption = "打开")]
    public class OpenCommand : CommonCommand<Process>
    {
        #region 属性
        /// <summary>
        /// 应用名
        /// </summary>
        private String appName;
        /// <summary>
        /// 文件名
        /// </summary>
        private String filename;
        /// <summary>
        /// x坐标
        /// </summary>
        private int x;
        /// <summary>
        /// y坐标
        /// </summary>
        private int y;
        /// <summary>
        /// 宽度
        /// </summary>
        private int w;
        /// <summary>
        /// 高度
        /// </summary>
        private int h;
        /// <summary>
        /// 显示方式
        /// </summary>
        private String showMode;
        /// <summary>
        /// 隐藏
        /// </summary>
        private bool hide;
        /// <summary>
        /// 等待退出
        /// </summary>
        private bool waitForExit;

        
        
        #endregion

        #region 显示和执行

        /// <summary>
        /// 字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "打开" + appName == null || appName == "" ? filename : appName + "(" + filename + ")"
                  + (showMode == null || showMode == "" ? "" : " " + showMode)
                  + (!hide ? "" : " hide")
                  + ((x <= 0 && y <= 0) ? "" : " x=" + x.ToString() + " y=" + y.ToString())
                  + ((w <= 0 && h <= 0) ? "" : " w=" + w.ToString() + " h=" + h.ToString());
        }
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(CommandContext context)
        {
            if (w == 0 && h == 0 && (showMode == null || showMode == ""))
                showMode = "max";
            if (w <= 0 && h != 0) w = Screen.PrimaryScreen.Bounds.Width;
            if (w != 0 && h <= 0) h = Screen.PrimaryScreen.Bounds.Height;
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (showMode == null) showMode = "";

            ProcessStartInfo MyStarInfo = new ProcessStartInfo();
            MyStarInfo.FileName = filename;
            ////窗口起始状态最大化
            if (showMode.ToLower() == "max")
                MyStarInfo.WindowStyle = ProcessWindowStyle.Maximized;
            else if (showMode.ToLower() == "min")
                MyStarInfo.WindowStyle = ProcessWindowStyle.Minimized;
            else
                MyStarInfo.WindowStyle = ProcessWindowStyle.Normal;

            if (hide)
                MyStarInfo.WindowStyle |= ProcessWindowStyle.Hidden;


            Process MyProcees = new Process();
            MyProcees.StartInfo = MyStarInfo;
            MyProcees.Start();

            if (x > 0 || y > 0 || w > 0 || h > 0)
            {
                Thread.Sleep(100);//让程序停一会
                Win32.MoveWindow(MyProcees.MainWindowHandle, x, y, w, h, true);
            }

            this.result = MyProcees;

            if (waitForExit)
                MyProcees.WaitForExit();
        }
        #endregion

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
                OpenCommand command = new OpenCommand();
                String cmdName = command.findNames(cmd);
                if (cmdName == null || cmdName == "")
                    return null;
                String cmdParam = cmd.Substring(cmdName.Length);
                cmdParam = cmdParam.Trim();
                int t1 = cmdParam.IndexOf("[");
                int t2 = cmdParam.IndexOf("]");
                int t3 = cmdParam.IndexOf(" ", t2 + 1);
                if(t1 < 0&& t3 < 0)
                {
                    command.filename = cmdParam;
                    cmdParam = "";
                }else if(t1<0)
                {
                    command.filename = cmdParam.Substring(0, t3);
                    cmdParam = cmdParam.Substring(t3 + 1);
                }else if(t2<0)
                {
                    msg = "OpenCommand参数错误:缺少右括号:" + cmdParam;
                    return null;
                }
                else
                {
                    command.appName = cmdParam.Substring(0, t1);
                    command.filename = cmdParam.Substring(t1 + 1, t2 - t1 - 1);
                    int len = command.appName.Length + command.filename.Length + 2;
                    cmdParam = t3 < 0 ? "" : cmdParam.Substring(t3 + 1);
                }

                if (!System.IO.File.Exists(command.filename))
                {
                    msg = "OpenCommand参数错误:文件不存在:" + cmdParam;
                    return null;
                }

                String[] totalParams = cmdParam.Split(' ');
                
                
                
                for (int i = 0; i < totalParams.Length; i++)
                {
                    if (totalParams[i] == null) continue;
                    totalParams[i] = totalParams[i].Trim();
                    if (totalParams[i] == "") continue;
                    if (totalParams[i].ToLower() == "max")
                        command.showMode = "max";
                    else if (totalParams[i].ToLower() == "min")
                        command.showMode = "min";
                    else if (totalParams[i].ToLower() == "hide")
                        command.hide = true;
                    else if (totalParams[i].ToLower() == "waitforexit")
                        command.waitForExit = true;
                    else if (totalParams[i].ToLower().Contains("x") && totalParams[i].ToLower().Contains("="))
                    {
                        int val = 0;
                        String[] ss = totalParams[i].Split('=');
                        if (ss != null && ss.Length >= 2 && int.TryParse(ss[1], out val))
                            command.x = val;
                    }
                    else if (totalParams[i].ToLower().Contains("y") && totalParams[i].ToLower().Contains("="))
                    {
                        int val = 0;
                        String[] ss = totalParams[i].Split('=');
                        if (ss != null && ss.Length >= 2 && int.TryParse(ss[1], out val))
                            command.y = val;
                    }
                    else if (totalParams[i].ToLower().Contains("w") && totalParams[i].ToLower().Contains("="))
                    {
                        int val = 0;
                        String[] ss = totalParams[i].Split('=');
                        if (ss != null && ss.Length >= 2 && int.TryParse(ss[1], out val))
                            command.w = val;
                    }
                    else if (totalParams[i].ToLower().Contains("h") && totalParams[i].ToLower().Contains("="))
                    {
                        int val = 0;
                        String[] ss = totalParams[i].Split('=');
                        if (ss != null && ss.Length >= 2 && int.TryParse(ss[1], out val))
                            command.h = val;
                    }
                }

                return command;
            }
        }
    }
}
