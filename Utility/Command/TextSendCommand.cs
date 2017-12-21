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
    [Text(Name = "Send", Caption = "发送")]
    public class TextSendCommand : CommonCommand
    {
        /// <summary>
        /// 发送内容 
        /// </summary>
        private String sendContent;
        /// <summary>
        /// 窗口变量名
        /// </summary>
        private String windowVariableName;        

        /// <summary>
        /// 字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "发送" + sendContent + "到" + windowVariableName;
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(CommandContext context)
        {
            IntPtr winHandle = (IntPtr)context.GetVariableValue(windowVariableName);
            if (IsExternal(sendContent))
                sendContent = (String)context.GetExternalValue(sendContent);
            if(winHandle == IntPtr.Zero)
            {
                for(int i=0;i< sendContent.Length;i++)
                {
                    SendKey((int)sendContent[i]);
                }
                return;
            }
            UnicodeEncoding encode = new UnicodeEncoding();
            char[] chars = encode.GetChars(encode.GetBytes(sendContent));
            Message msg;
            foreach (char c in chars)
            {
                msg = Message.Create(winHandle, Win32.WM_CHAR, new IntPtr(c), new IntPtr(0));
                Win32.PostMessage(msg.HWnd, msg.Msg, msg.WParam, msg.LParam);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public void SendKey(int vk)
        {
            Win32.keybd_event((byte)vk, 0, 0, 0);
            Win32.keybd_event((byte)vk, 0, 2, 0);
            /*Win32.SendMessage(hookedWindow, Win32.WM_KEYDOWN, vk, 0);//发
            Win32.SendMessage(hookedWindow, Win32.WM_CHAR, vk, 0); //回车
            Win32.SendMessage(hookedWindow, Win32.WM_KEYUP, vk, 0); //送
           */
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
                TextSendCommand command = new TextSendCommand();
                String cmdName = command.findNames(cmd);
                if (cmdName == null || cmdName == "")
                    return null;
                String cmdParam = cmd.Substring(cmdName.Length);

                int t = cmdParam.IndexOf("到");
                command.sendContent = cmdParam.Substring(0, t);
                command.windowVariableName = cmdParam.Substring(t + 1);
                if (!command.sendContent.NotEmpty())
                    throw new Exception("文本内容无效:"+cmd);
                if(!command.windowVariableName.NotEmpty())
                    throw new Exception("文本窗口变量名无效:" + cmd);

                return command;
            }
        }

    }
}