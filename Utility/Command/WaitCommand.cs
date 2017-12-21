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
    [Text(Name="Wait",Caption = "等待", Alias = new String[]{ "暂停","休眠","sleep"})]
    public class WaitCommand : CommonCommand
    {
        
        /// <summary>
        /// 时间单位
        /// </summary>
        private TimeUnit timeUnit;
        /// <summary>
        /// 时间值
        /// </summary>
        private double time;

        public override string ToString()
        {
            return "Wait " + time + timeUnit.ToString();
        }
        public override void Execute(CommandContext context)
        {
            int ms = TimeUnitUtils.Transfer((int)time, timeUnit, TimeUnit.millsecond);
            System.Threading.Thread.Sleep(ms);
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
                WaitCommand command = new WaitCommand();
                String cmdName = command.findNames(cmd);
                if (cmdName == null || cmdName == "")
                    return null;
                String cmdParam = cmd.Substring(cmdName.Length);
                int[] numberpos = new int[2];
                int number = TextUtils.FeatchNumber(cmdParam, numberpos);
                if(numberpos[0]  == -1)
                {
                    msg = "无法解析命令WaitCommand的参数:缺少时间项" + cmdParam;
                    return null;
                }
                String timeUnitStr = cmdParam.Substring(numberpos[1]+1);

                TimeUnit timeUnit = new TimeUnit();
                try
                {
                    timeUnit = TimeUnitUtils.Parse(timeUnitStr);
                }
                catch (Exception e)
                {
                    msg = "无法解析命令WaitCommand的参数:时间单位无效" + e.Message + ":" + cmdParam;
                    return null;
                }

                command.timeUnit = timeUnit;
                command.time = number;


                
                if (number<0)
                {
                    msg = "无法解析命令WaitCommand的参数:时间值错误:" + number + ":" + cmdParam;
                    return null;
                }
                

                return command;
            }
        }
    }
}
