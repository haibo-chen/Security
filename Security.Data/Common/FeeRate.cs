using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Utility.Date;
namespace insp.Security.Data.Common
{
    /// <summary>
    /// 费率信息
    /// </summary>
    public class FeeRate
    {        
        protected List<DateSection<double>> stamps = new List<DateSection<double>>();
        protected double defaultValue = 0.001;
        public double this[DateTime date]
        {
            get
            {
                DateSection<double> ds = stamps.FirstOrDefault<DateSection<double>>(x => x.isIn(date));
                return ds == null ? defaultValue : ds.Data;
            }
        }
    }
}
