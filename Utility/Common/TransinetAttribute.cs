using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Utility.Common
{
    /// <summary>
    /// 透明属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field| AttributeTargets.Property, Inherited = true)]
    public class TransinetAttribute : Attribute
    {
    }
}
