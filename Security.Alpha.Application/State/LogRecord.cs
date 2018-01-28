using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Date;
using insp.Utility.Collections;

namespace Security.Alpha.Application.State
{
    /// <summary>
    /// 状态记录
    /// </summary>
    public class LogRecord : IComparable<LogRecord>
    {
        #region 常量
        public const String EVENT_DOWNLOADING = "下载日线";
        public const String EVENT_REPOSITORY = "日线数据入库";
        public const String EVENT_GENERATE = "生成指标";
        public const String EVENT_BUY = "选择可卖出股票";
        public const String EVENT_SELL = "选择可买入股票";        
        public const String EVENT_ENTER = "等待确认";
        public const String EVENT_WAIT_TRADE = "等待下单";
        public const String EVENT_DO_TRADE = "执行下单";
        public readonly List<String> EVENTS = CollectionUtils.AsList(EVENT_DOWNLOADING, EVENT_REPOSITORY, EVENT_GENERATE, EVENT_SELL, EVENT_BUY, EVENT_WAIT_TRADE, EVENT_DO_TRADE);
        #endregion

        #region 事件记录属性
        public readonly DateTime begintime;
        public readonly String eventName;
        public String data="";
        public String description = "";

        public DateTime endtime = DateUtils.InitDate;
        public int error = 0;
        public String msg = "";
        public String detailMsg = "";
        #endregion

        #region 初始化
         
        public LogRecord(DateTime time, String eventName, String data, String description)
        {
            this.begintime = endtime = time;
            this.eventName = eventName;
            this.data = data;
            this.description = description;
        }

        public void recordEnd(String msg, int error = 0, String detailMsg = "")
        {
            this.endtime = DateTime.Now;
            this.msg = msg;
            this.error = error;
            this.detailMsg = detailMsg;

        }
        #endregion

        #region 读写
        public static LogRecord Parse(String s)
        {
            if (s == null || s.Trim() == "") return null;
            String[] ss = s.Split(',');
            if (ss == null || ss.Length <= 0) return null;

            DateTime time = DateTime.ParseExact(ss[0], DateUtils.FMT_DATETIME_DEFAULT, null);
            String eventName = ss.Length<2 || ss[1] == null ? "" : ss[1].Trim();
            String data = ss.Length < 3 || ss[2] == null ? "" : ss[2].Trim();
            String desc = ss.Length < 4 || ss[3] == null ? "" : ss[3].Trim();

            DateTime endtime = ss.Length < 5 || ss[4] == null ? time : DateTime.ParseExact(ss[4], DateUtils.FMT_DATETIME_DEFAULT, null);
            int error = ss.Length < 6 || ss[5] == null ? 0 : int.Parse(ss[5]);
            String msg = ss.Length < 7 || ss[6] == null ? "" : ss[6];
            String detailMsg = ss.Length < 8 || ss[7] == null ? "" : ss[7];
            LogRecord r = new LogRecord(time, eventName, data, desc);
            r.endtime = endtime;
            r.error = error;
            r.msg = msg;
            r.detailMsg = detailMsg;

            return r;
        }

        

        public override string ToString()
        {
            return begintime.ToString(DateUtils.FMT_DATETIME_DEFAULT) + "," + eventName +
                   "," + data +
                   "," + description +
                   "," + (this.endtime.ToString(DateUtils.FMT_DATETIME_DEFAULT)) +
                   "," + error.ToString() +
                   "," + msg +
                   "," + detailMsg;
        }

        public int CompareTo(LogRecord other)
        {
            if (other == null) return 1;
            return (int)(this.begintime - other.begintime).TotalMilliseconds;
        }

        #endregion
    }
}
