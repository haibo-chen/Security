using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;
using insp.Utility.Collections;
using insp.Utility.Collections.Tree;
using insp.Utility.Collections.Time;

using insp.Security.Data;
using insp.Security.Data.kline;
using insp.Security.Data.Indicator;

using insp.Utility.IO;
using insp.Utility.Bean;
using insp.Utility.Common;

using insp.Security.Strategy;

namespace Security.Alpha4.Test
{
    public class TestCaseBase
    {
        public DateTime begin = new DateTime(2017, 1, 6);
        public DateTime end = new DateTime(2017, 9, 30);
        public double funds = 50000;

        
   
        
    }
}
