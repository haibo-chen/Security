using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using insp.Utility.IO;

namespace insp.Utility.Reflection
{
    public class TypeUtils
    {
        public static Assembly LoadAssemby(String name,String path="")
        {
            if (name == null || name == "") return null;
            Assembly assembly = FindAssembly(name);
            if (assembly != null)
                return assembly;
            if (!Directory.Exists(path))
                path = FileUtils.GetDirectory();
            if (!path.EndsWith("\\")) path += "\\";
            return Assembly.LoadFile(path + name);
        }
        public static Assembly FindAssembly(String name)
        {
            if (name == null || name == "")
                return null;
            Assembly[] assemblys = AppDomain.CurrentDomain.GetAssemblies();
            if (assemblys == null || assemblys.Length <= 0)
                return null;
            foreach(Assembly assembly in assemblys)
            {
                if (assembly.FullName.ToLower() == name.ToLower())
                    return assembly;
                if (assembly.GetName().Name.ToLower() == name.ToLower())
                    return assembly;
            }
            return null;
        }
    }
}
