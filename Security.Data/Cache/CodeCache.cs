using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Data.Cache
{
    public class CodeCache
    {
        /// <summary>
        /// 代码
        /// </summary>
        public readonly String Code;

        protected Dictionary<Type, Block> blocks = new Dictionary<Type, Block>();
    }
}
