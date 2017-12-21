using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Security.Data.Security
{
    /// <summary>
    /// 证劵属性信息集
    /// </summary>
    public class SecurityPropertiesSet : Dictionary<String, SecurityProperties>
    {
        /// <summary>
        /// 所有股票代码
        /// </summary>
        public List<String> Codes { get { List<String> r = this.Keys.ToList();r.Sort();return r; } }
        /// <summary>
        /// 合并
        /// </summary>
        /// <param name="spList"></param>
        public void Merge(List<SecurityProperties> spList)
        {
            if (spList == null || spList.Count <= 0)
                return;
            spList.ForEach(x =>
            {
                if (this.ContainsKey(x.Code))
                    this[x.Code] = x;
                else
                    this.Add(x.Code, x);
            });
        }

        public List<SecurityProperties> ToList()
        {
            List<SecurityProperties> r = new List<SecurityProperties>();
            Codes.ForEach(x => r.Add(this[x]));
            return r;
        }
        
    }
}
