using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

using insp.Utility.Text;

namespace insp.Utility.Command
{
    /// <summary>
    /// 命令解释器
    /// </summary>
    public class CommandInterpreter
    {
        #region 属性
        public const String COMMENT = "#";
        /// <summary>
        /// 日志
        /// </summary>
        log4net.ILog logger = log4net.LogManager.GetLogger("Command");
        /// <summary>
        /// 原始命令行
        /// </summary>
        private String[] orginLines;
        /// <summary>
        /// 命令信息
        /// </summary>
        private List<CommandInfo> commandInfos = new List<CommandInfo>();
        /// <summary>
        /// 循环信息
        /// </summary>
        private List<ForInfo> loops = new List<ForInfo>();
        /// <summary>
        /// 上下文
        /// </summary>
        private CommandContext context = new CommandContext();
        
        #endregion

        #region 初始化
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="lines"></param>
        public CommandInterpreter(String[] lines,CommandContext context)
        {
            orginLines = lines;
            this.context = context;
            if (this.context == null)
                this.context = new CommandContext();
        }
        /// <summary>
        /// 从文件中读取
        /// </summary>
        /// <param name="filename"></param>
        public CommandInterpreter(String filename, CommandContext context)
        {
            orginLines = System.IO.File.ReadAllLines(filename);
            this.context = context;
            if (this.context == null)
                this.context = new CommandContext();

        }
        #endregion
        /// <summary>
        /// 语法检查
        /// </summary>
        public CommandInterpreter Parse()
        {
            Dictionary<String, Object> map = context.ExternalVariable;

            commandInfos.Clear();
            if (map != null && map.Count > 0)
                context.SetExternal(map);

            ForInfo forInfo = null;
            for (int i=0;i< orginLines.Length;i++)
            {
                if (orginLines[i] == null) continue;
                String line = orginLines[i].Trim();
                if (line == "") continue;
                if (line.StartsWith(COMMENT)) continue;
                int t = line.IndexOf(COMMENT);
                if (t >= 0)
                    line = line.Substring(0, t).Trim();

                String variableName = parseVariableName(line,i);
                if (forInfo == null)
                {
                    forInfo = parseForBegin(line, i);
                    if (forInfo != null) continue;
                }
                else if (parseForEnd(line, i, ref forInfo))
                    continue;

                String commandStr = line;
                if (variableName != "")
                    commandStr = line.Substring(line.IndexOf("=") + 1).Trim();


                ICommand command = null;
                try
                {
                    command = context.Receive(commandStr);
                    if (command == null)
                        throw new Exception("第" + i + "行语法错误:无法识别的命令:" + line);
                }
                catch (Exception e)
                {
                    throw new Exception("第" + i + "行语法错误:" + e.Message);
                }
                CommandInfo cmdInfo = new CommandInfo(i, line, command, variableName);
                this.commandInfos.Add(cmdInfo);

                
            }
            return this;
        }

        
        /// <summary>
        /// 从line中取得变量名
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private String parseVariableName(String line,int i)
        {
            if (!line.StartsWith("$"))//开头是变量
                return "";
            int t1 = line.IndexOf('=');
            if (t1 < 0)
                throw new Exception("第" + i + "行语法错误:变量没有赋值:"+line);
            return line.Substring(0, t1).Trim();            
        }
        /// <summary>
        /// 解析循环开始符号
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private ForInfo parseForBegin(String line, int i)
        {
            if (!line.ToLower().StartsWith("for"))
                return null;
            String[] ss = line.Split(' ', ':');
            if (ss == null || ss.Length != 3)
                throw new Exception("第" + i + "行语法错误:for语句无效:" + line);
            if (ss[2] == null || ss[2].Trim() == "")
                throw new Exception("第" + i + "行语法错误:for语句集合错误:" + line);
            
            return new ForInfo() { begin = i, elementVarialbeName  = "{$" + ss[1] + "}",listVariableName = "{$" + ss[2] + "}"};            
        }
        /// <summary>
        /// 解析循环结束符号
        /// </summary>
        /// <param name="line"></param>
        /// <param name="i"></param>
        /// <param name="forInfo"></param>
        /// <returns></returns>
        private bool parseForEnd(String line, int i, ref ForInfo forInfo)
        {
            if (!line.ToLower().StartsWith("endfor"))
                return false;
            forInfo.end = i;
            return true;
        }

        

        #region 执行
        public void Execute()
        {            
            Dictionary<String, Object> map = context.ExternalVariable;
            if (map == null)
                map = new Dictionary<string, object>();
            if (map != null && map.Count > 0)
                context.SetExternal(map);

            ForInfo curFor = null;
            int curForIndex = -1;
            logger.Info("命令集开始执行...");
            for (int i=0;i< commandInfos.Count;i++)
            {
                //取得当前命令
                CommandInfo curCommandInfo = commandInfos[i];
                //判断当前命令是否在循环内
                if (curFor == null)
                {
                    curFor = getFor(curCommandInfo);
                    curForIndex = curFor == null ? -1 : i;
                }
                //如果在循环内，设置循环变量直到结束
                if (curFor != null && !curFor.setNextElementVariable(context))
                {
                    i = getForAfterIndex(curFor); //调到循环后面
                    curFor = null;
                    curForIndex = -1;
                    continue;
                } 
            

                //执行命令
                try
                {
                    curCommandInfo.command.Execute(context);
                    Object r = curCommandInfo.command.GetResult();
                    logger.Info("执行"+curCommandInfo.ToString()+"完成"+ (r==null?"":":"+r.ToString()));
                }
                catch(Exception e)
                {
                    logger.Error("执行命令失败:"+ curCommandInfo.ToString()+":"+e.Message);
                    return;
                }
                //命令结果赋值
                if (curCommandInfo.variableName.NotEmpty())
                    context.PutVariableValue(curCommandInfo.variableName, curCommandInfo.command.GetResult());

                //当前在循环，且循环体到了最后
                if(curFor!=null && IsLastCommand(curFor,curCommandInfo))
                {
                    i = curForIndex;
                    continue;
                }

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private ForInfo getFor(CommandInfo command)
        {
            if (command == null) return null;
            foreach(ForInfo loop in this.loops)
            {
                if (command.orginLine > loop.begin &&
                   command.orginLine < loop.end)
                {
                    IList list = (IList)context.GetExternalValue(loop.listVariableName);
                    if (list == null)
                        throw new Exception(loop.ToString()+"集合变量缺少有效值:"+ loop.listVariableName);
                    loop.loopXh = -1;                    
                    return loop;
                }
                    
            }
            return null;
        }
        /// <summary>
        /// command是否是loop中的最后一个命令
        /// </summary>
        /// <param name="loop"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private bool IsLastCommand(ForInfo loop, CommandInfo command)
        {
            for(int i=command.orginLine+1;i<loop.end;i++)
            {
                if (commandInfos.FirstOrDefault(x => x.orginLine == i) != null)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// 查找for循环后面的有效命令行号
        /// </summary>
        /// <param name="loop"></param>
        /// <returns></returns>
        private int getForAfterIndex(ForInfo loop)
        {
            int i = loop.end + 1;
            while(i< orginLines.Length)
            {
                for(int k=0;k< commandInfos.Count;k++)
                {
                    if (commandInfos[k].orginLine >= i)
                        return k;
                }
                i += 1;
            }
            return commandInfos.Count;
        }
        #endregion

        #region 内部类
        private class ForInfo
        {
            /// <summary>
            /// 开始位置
            /// </summary>
            public int begin;
            /// <summary>
            /// 结束位置
            /// </summary>
            public int end;
            /// <summary>
            /// 集合变量名
            /// </summary>
            public String listVariableName;
            /// <summary>
            /// 元素变量名
            /// </summary>
            public String elementVarialbeName;
            /// <summary>
            /// 循环序号 
            /// </summary>
            public int loopXh = -1;

            public bool setNextElementVariable(CommandContext context)
            {
                IList list = (IList)context.GetExternalValue(listVariableName);
                if (loopXh+1 >= list.Count)
                {
                    loopXh = -1;
                    return false;
                }
                loopXh += 1;
                Object element = list[loopXh];
                context.PutExternalValue(elementVarialbeName, element);
                return true;
            }
        }
        /// <summary>
        /// 命令信息
        /// </summary>
        class CommandInfo
        {
            /// <summary>行号</summary>        
            public int orginLine;
            /// <summary>命令字符串</summary> 
            public String commandString;
            /// <summary>命令</summary> 
            public ICommand command;
            /// <summary>变量名</summary> 
            public String variableName;
            /// <summary>
            /// 构造方法
            /// </summary>
            /// <param name="orginLine"></param>
            /// <param name="commandString"></param>
            /// <param name="command"></param>
            /// <param name="variableName"></param>
            public CommandInfo(int orginLine, String commandString, ICommand command, String variableName)
            {
                this.orginLine = orginLine;
                this.commandString = commandString;
                this.command = command;
                this.variableName = variableName;
            }
            public override string ToString()
            {
                return "第" + this.orginLine + "行命令:" + commandString;
            }
        }

        #endregion

    }

    
}
