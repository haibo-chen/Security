using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using insp.Utility.Bean;


namespace insp.Utility.IO
{
    public class CSVFile
    {
        private PropertyDescriptorCollection pdc = new PropertyDescriptorCollection();
        public CSVFile() { }
        public CSVFile(PropertyDescriptorCollection pdc) { this.pdc = pdc; }
        public CSVFile(params PropertyDescriptor[] pdcs)
        {
            this.pdc.AddRange(pdcs);
        }

        private String sep;
        private String title = "";
        private List<String> lines = new List<string>();
        public List<String> Lines { get { return lines; } }
        public int LineCount { get { return lines.Count; } }
        public T Get<T>(int line,int col,T defaultValue=default(T))
        {
            if (lines.Count <= line) return defaultValue;
            String row = lines[line];
            String[] ss = row.Split(sep.ToArray());
            if (ss == null || ss.Length <= col)
                return defaultValue;
            String val = ss[col];

            PropertyDescriptor pd = pdc.Count >= col ? null : pdc[col];
            return ConvertUtils.ConvertTo<T>(val, pd == null ? "" : pd.format, null);
        }


        public int Load(String filename,Encoding encoding,bool hasTitle,String sep)
        {
            this.sep = sep;
            if (!File.Exists(filename))
                return -1;
            this.lines.Clear();
            String[] lines = File.ReadAllLines(filename,encoding);
            if (lines == null || lines.Length <= 0)
                return 0;
            this.lines.AddRange(lines);
            title = "";
            if(hasTitle)
            {
                title = this.lines[0];
                this.lines.RemoveAt(0);
            }
            return this.lines.Count;
        }
    }
}
