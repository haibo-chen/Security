using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Trade
{
    /// <summary>
    /// 委托参数
    /// </summary>
    public class EntrustParam
    {
        public readonly int retryNum = 0;
        public readonly int maxQueryNum = 0;
        public readonly int queryInterval = 0;
        public readonly EntrustType entrustType = EntrustType.LimitOrder;

        public EntrustParam() { }
        public EntrustParam(int retryNum, int maxQueryNum, EntrustType entrustType)
        {
            this.retryNum = retryNum;
            this.maxQueryNum = maxQueryNum;
            this.entrustType = entrustType;
        }
    }
    /// <summary>
    /// 委托请求
    /// </summary>
    public class EntrustRequest
    {
        public readonly String code;
        public readonly double amount;
        public readonly double price;
        public readonly String reason;
        
        public EntrustRequest(String code, double amount, double price,String reason="")
        {
            this.code = code;
            this.amount = amount;
            this.price = price;
            this.reason = reason;
        }
        


    }
}
