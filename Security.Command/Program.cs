using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


using insp.Utility.Collections;
using insp.Utility.Collections.Tree;
using insp.Utility.Collections.Time;

using insp.Security.Data;
using insp.Security.Data.kline;
using insp.Security.Data.Indicator;

using insp.Utility.IO;
using insp.Utility.Bean;

using insp.Security.Strategy;


namespace insp.Security.Command
{
    


    class Program
    {        
        static void Main(string[] args)
        {

            //generateData();
            new BacktestEngine().Execute();
        }

        static void generateData()
        {
            String filename = "stocks.txt", path = FileUtils.GetDirectory();
            if (filename == null || filename == "")
                filename = "stocks.txt";

            String fullfilename = path + filename;// "stocks.txt";
            if (!File.Exists(fullfilename))
                fullfilename = FileUtils.GetDirectory() + filename;
            List<String> strs = new List<string>();
            strs.AddRange(System.IO.File.ReadAllLines(filename).Select(x => x));
            strs = strs.ConvertAll(x => x.Split(',')[1].Trim());

            String datapath = "c:\\TXTDAY2XMA\\";
            String resultpath = datapath;
            Parallel.ForEach(strs, code =>
            {
                Strategy.Alpha.StrategyDataSet.CreateOrLoad(code, datapath, resultpath, false, "1");
            });
            
        }
    }
}
