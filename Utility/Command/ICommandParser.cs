using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Utility.Command
{
    /// <summary>
    /// 命令解析器
    /// </summary>
    public interface ICommandParser
    {
        /// <summary>
        /// 解析命令字符串
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="msg">如果解析命令有错误，这里是错误信息</param>
        /// <returns>解析得到的命令</returns>
        ICommand TryParse(String cmd,out String msg);
    }
}
