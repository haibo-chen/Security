using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Date;
using System.Diagnostics;

namespace insp.Utility.Command
{
    /// <summary>
    /// 命令执行记录
    /// </summary>
    public class ExecuteRecord
    {
        #region 时间
        /// <summary>
        /// 开始时间
        /// </summary>
        private DateTime begin;
        /// <summary>
        /// 结束时间
        /// </summary>
        private DateTime end;
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime Begin { get { return begin; } }
        /// <summary>
        /// 开始时间
        /// </summary>
        public String BeginStr { get { return DateUtils.ToText(begin); } }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime End { get { return end; } }
        /// <summary>
        /// 结束时间
        /// </summary>
        public String EndStr { get { return DateUtils.ToText(end); } } 
        /// <summary>
        /// 时间跨度
        /// </summary>
        public TimeSpan TimeSpan { get { return end - begin; } }
        #endregion

        #region 结果
        /// <summary>错误号</summary>         
        private int error = 0;
        /// <summary>错误信息</summary>        
        private String msg = "";
        /// <summary>详细信息</summary>        
        private String detailMsg = "";
        #endregion

        #region 记录
        /// <summary>
        /// 记录开始
        /// </summary>
        /// <returns></returns>
        public static ExecuteRecord DoBegin()
        {
            ExecuteRecord r = new ExecuteRecord();
            r.begin = DateTime.Now;
            r.end = DateTime.Now;
            return r;
        }
        /// <summary>
        /// 记录结束
        /// </summary>
        /// <param name="error"></param>
        /// <param name="msg"></param>
        /// <param name="detailMsg"></param>
        public void DoEnd(int error=0,String msg="",String detailMsg="")
        {
            end = DateTime.Now;
            this.error = error;
            this.msg = msg;
            this.detailMsg = detailMsg;
        }
        #endregion

    }
    /// <summary>
    /// 执行记录
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExecuteRecord<T> : ExecuteRecord
    {
        /// <summary>结果</summary>         
        public T result;
        /// <summary>结果</summary>  
        public T Result { get { return result; } }
        /// <summary>
        /// 记录结束
        /// </summary>
        /// <param name="result"></param>
        /// <param name="error"></param>
        /// <param name="msg"></param>
        /// <param name="detailMsg"></param>
        public void DoEnd(T result, int error = 0, String msg = "", String detailMsg = "")
        {
            base.DoEnd(error, msg, detailMsg);
            this.result = result;
        }
    }
}
