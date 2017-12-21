using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using insp.Security.Data.kline;
using insp.Utility.Collections.Time;
using insp.Security.Data.Indicator.Fund;
using insp.Security.Data.Indicator;

namespace insp.Security.Data
{
    public interface IIndicatorContext
    {
        
        /// <summary>
        /// 取得K线
        /// </summary>
        /// <param name="code"></param>
        /// <param name="timeunit">时间单位</param>
        /// <param name="year">年份，为0表示取得所有</param>
        /// <returns></returns>
        KLine GetKline(String code,TimeUnit timeunit= TimeUnit.day,int year=0);
        /// <summary>
        /// 取得资金动向指标
        /// </summary>
        /// <param name="code"></param>
        /// <param name="timeunit"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        MovementOfFunds GetMovementOfFunds(String code, TimeUnit timeunit = TimeUnit.day, int year = 0);
        /// <summary>
        /// 取得立体买卖指标
        /// </summary>
        /// <param name="code"></param>
        /// <param name="timeunit"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        TradingLine GetTradingLine(String code, TimeUnit timeunit = TimeUnit.day, int year = 0);
    }
}
