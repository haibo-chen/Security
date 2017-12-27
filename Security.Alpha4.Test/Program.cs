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
using insp.Security.Strategy.Alpha;
using insp.Security.Data.Security;

namespace Security.Alpha4.Test
{
    class Program
    {

        static void Main(string[] args)
        {
            StrategyContext context = new StrategyContext();
            context.DoTest();


            /*StrategyFactory factory = new StrategyFactory();
            factory.Put(new AlphaStrategy5())
                   .Put(new AlphaStrategy6())
                   .Put(new AlphaStrategy7());



            Properties prop = Properties.Load(FileUtils.GetDirectory() + "alpha7.properties", Encoding.UTF8);

            IndicatorRepository repository = new IndicatorRepository(prop.Get<String>("backtest.datapath"));
            repository.Initilization();
            
            prop.Put("backtest.repository", repository);*/

            //new BacktestContext(prop, factory).Build().Run();
        }

    } 
}
