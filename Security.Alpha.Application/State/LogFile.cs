using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using insp.Utility.IO;
using insp.Utility.Date;

namespace Security.Alpha.Application.State
{
    /// <summary>
    /// 状态文件
    /// </summary>
    public class LogFile
    {
        /// <summary>
        /// 日期
        /// </summary>
        public readonly DateTime date;
        /// <summary>
        /// 记录
        /// </summary>
        public List<LogRecord> records = new List<LogRecord>();
        /// <summary>
        /// 当前状态
        /// </summary>
        public LogRecord CurrentState
        {
            get { return records == null || records.Count <= 0 ? null : records.Last(); }
            set
            {
                if (CurrentState == value) return;
                this.records.Add(value);
                Save();
            }
        }

        public LogFile(DateTime date)
        {
            this.date = date;
            Load();
        }

        public String GetFileName()
        {
            return FileUtils.GetDirectory("records") + date.ToString(DateUtils.FMT_DATE_COMPACT) + ".state";
        }
        /// <summary>
        /// 保存文件
        /// </summary>
        public void Save()
        {
            File.WriteAllLines(GetFileName(),
                               records.ConvertAll(x => x.ToString()).ToArray());
        }

        public LogRecord Load()
        {
            String filename = GetFileName();
            if (!File.Exists(filename))
                return null;
            String[] lines = File.ReadAllLines(GetFileName());
            if (lines == null || lines.Length <= 0) return null;

            this.records.Clear();
            lines.ToList().ForEach(x => { LogRecord r = LogRecord.Parse(x); if (r != null) records.Add(r); });

            records.Sort();
            return CurrentState;
        }
    }
}
