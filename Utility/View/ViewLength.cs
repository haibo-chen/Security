using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Utility.View
{
    public enum LengthType
    {
        Percent,PX
    }
    public class ViewLength
    {
        public LengthType Type { get; set; }
        public double Value { get; set; }
    }
}
