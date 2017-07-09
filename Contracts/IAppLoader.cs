using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Startup.Contracts
{
    public interface IAppLoader
    {
        void Run();
        void Run(Assembly assembly);
    }
}
