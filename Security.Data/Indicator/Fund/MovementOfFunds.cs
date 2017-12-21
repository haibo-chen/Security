using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Bean;
using insp.Utility.Collections.Time;

using insp.Security.Data.kline;

namespace insp.Security.Data.Indicator.Fund
{
    /// <summary>
    /// 资金动向
    /// </summary>
    public class MovementOfFunds : IIndicator
    {
        /// <summary>
        /// 主力动向 
        /// </summary>
        public TimeSeries<ITimeSeriesItem<double>> mainForce;
        /// <summary>
        /// 散户动向
        /// </summary>
        public TimeSeries<ITimeSeriesItem<double>> retailInvestors;

    }
}
