using insp.Utility.Bean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Strategy
{
    public interface ITradeOperator
    {
        String Name { get; set; }
        String Caption { get; set; }
    }
    public interface IBuyer : ITradeOperator
    {

        List<TradeBout> Execute(String code, Properties strategyParam, BacktestParameter backtestParam, ISeller seller = null);
    }
    public interface ISeller : ITradeOperator
    {
        void Execute(List<TradeBout> bouts, Properties strategyParam, BacktestParameter backtestParam);
    }

    public abstract class TradeOperator : ITradeOperator
    {
        protected String name;
        public String Name { get { return name; } set { name = value; } }

        protected String caption;
        public String Caption { get { return caption; } set { caption = value; } }

        protected PropertyDescriptorCollection pdc = new PropertyDescriptorCollection();
        public PropertyDescriptorCollection PDC { get { return pdc; } }
        public List<PropertyDescriptor> PDList { get { return pdc.ToList(); } set { pdc.Clear(); pdc.AddRange(value); } }

        public override string ToString()
        {
            return Name + "," + Caption;
        }
    }

    public abstract class Buyer : TradeOperator,IBuyer
    {        
        public abstract List<TradeBout> Execute(string code, Properties strategyParam, BacktestParameter backtestParam, ISeller seller = null);
    }
    public abstract class Seller : TradeOperator,ISeller
    {        
        public abstract void Execute(List<TradeBout> bouts, Properties strategyParam, BacktestParameter backtestParam);
    }
}
