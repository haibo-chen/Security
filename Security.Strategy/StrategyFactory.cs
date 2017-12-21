using insp.Utility.Bean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Strategy
{
    /// <summary>
    /// 策略工厂
    /// </summary>
    public class StrategyFactory 
    {
        private List<IStrategyMeta> metas = new List<IStrategyMeta>();

        public StrategyFactory Put(IStrategyMeta meta)
        {
            this.metas.Add(meta);
            return this;
        }
        public IStrategyMeta this[String name]
        {
            get { return metas.FirstOrDefault(x => x.HasName(name)); }
            set { int index = metas.IndexOf(this[name]);if (index < 0) this.metas.Add(value); else this.metas[index] = value;  }
        }
    }
}
