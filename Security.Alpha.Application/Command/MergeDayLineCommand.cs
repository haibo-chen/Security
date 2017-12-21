using insp.Utility.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using insp.Utility.Text;
using insp.Utility.Date;
using insp.Utility.Bean;
using insp.Utility.Collections.Time;

using insp.Security.Data.kline;
using insp.Security.Data;

namespace Security.Alpha.Application.Command
{
    /// <summary>
    /// 合并日线命令
    /// 每天下载的日线数据合并到总日线数据中去
    /// </summary>
    [Text(Name = "MergeDayLine", Caption = "合并日线")]
    public class MergeDayLineCommand : CommonCommand
    {
        /// <summary>
        /// 每天日线路径
        /// </summary>
        private String everydayLinePath;
        /// <summary>
        /// 日线仓库路径
        /// </summary>
        private String dayLineReposorityPath;
        /// <summary>
        /// 合并日期
        /// </summary>
        private String mergeDateString;
        /// <summary>
        /// 合并日期
        /// </summary>
        private DateTime mergeDate;

        /// <summary>
        /// 字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "合并日线";
        }

        /// <summary>
        /// 执行方法 
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(CommandContext context)
        {
            logger.Debug("执行命令:" + this.ToString());
            //取得参数
            everydayLinePath = (String)context.GetExternalValue(everydayLinePath);
            dayLineReposorityPath = (String)context.GetExternalValue(dayLineReposorityPath);
            mergeDateString = (String)context.GetExternalValue(mergeDateString);
            mergeDate = DateUtils.Parse(mergeDateString);
            logger.Debug("取得参数:源路径=" + everydayLinePath + ",合并路径=" + dayLineReposorityPath + ",日期=" + mergeDateString);
            if (!everydayLinePath.EndsWith("\\")) everydayLinePath += "\\";
            if (!dayLineReposorityPath.EndsWith("\\")) dayLineReposorityPath += "\\";

            IndicatorRepository repository = (IndicatorRepository)context.GetExternalValue("{$repository}");

            //检查路径
            DirectoryInfo dFromInfo = new DirectoryInfo(everydayLinePath);
            if (!dFromInfo.Exists)
                throw new Exception("单日日线存储路径不存在:" + everydayLinePath);

            DirectoryInfo dToInfo = new DirectoryInfo(dayLineReposorityPath);
            if (!dToInfo.Exists)
                throw new Exception("日线存储仓库路径不存在:" + dayLineReposorityPath);

            FileInfo[] finfos = dFromInfo.GetFiles("*.txt");
            logger.Debug("共有" + (finfos == null ? "0" : finfos.Length.ToString()) + "个单日K线数据待合并...");
            foreach (FileInfo fInfo in finfos)
            {
                logger.Debug("处理" + fInfo.Name + "...");
                String market = fInfo.Name.Substring(0, fInfo.Name.IndexOf("#"));
                String code = fInfo.Name.substring(0, "#", ".");
                String[] lines = File.ReadAllLines(fInfo.FullName);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i] == null || lines[i] == "") continue;
                    if (!lines[i].Contains(",")) continue;
                    String[] ss = lines[i].Trim().Split(',');
                    if (ss == null || ss.Length != 7) continue;
                    if (ss[0] == null) continue;
                    DateTime d = DateUtils.InitDate;
                    if (!DateUtils.TryParse(ss[0].Trim(), out d))
                        continue;
                    if (d.Date != this.mergeDate.Date)
                        continue;
                    KLineItem item = new KLineItem();
                    item.SetValue<String>(KLineItem.PD_CODE.Name, code);
                    item.SetValue<DateTime>(KLineItem.PD_TIME.Name, d);
                    item.SetValue<double>(KLineItem.PD_OPEN.Name, ConvertUtils.ConvertTo<double>(ss[1].Trim()));
                    item.SetValue<double>(KLineItem.PD_HIGH.Name, ConvertUtils.ConvertTo<double>(ss[2].Trim()));
                    item.SetValue<double>(KLineItem.PD_LOW.Name, ConvertUtils.ConvertTo<double>(ss[3].Trim()));
                    item.SetValue<double>(KLineItem.PD_CLOSE.Name, ConvertUtils.ConvertTo<double>(ss[4].Trim()));
                    item.SetValue<double>(KLineItem.PD_VOLUMN.Name, ConvertUtils.ConvertTo<double>(ss[5].Trim()));
                    item.SetValue<double>(KLineItem.PD_TURNOVER.Name, ConvertUtils.ConvertTo<double>(ss[6].Trim()));

                    TimeSerialsDataSet ds = repository[code];
                    if (ds == null)
                        continue;
                    ds.DayKLine[d.Date] = item;
                    ds.Save("kline", TimeUnit.day);
                    logger.Debug("合并到" + market.ToUpper() + code + ".csv");
                }
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
                MergeDayLineCommand command = new MergeDayLineCommand();
                String cmdName = command.findNames(cmd);
                if (cmdName == null || cmdName == "")
                    return null;
                String cmdParam = cmd.Substring(cmdName.Length);
                return command;
            }
        }
    }
}