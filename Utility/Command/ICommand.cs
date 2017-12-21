using insp.Utility.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insp.Utility.Command
{
    public interface ICommand : INamed
    {
        void Execute(CommandContext context);
        Object GetResult();
    }
}
