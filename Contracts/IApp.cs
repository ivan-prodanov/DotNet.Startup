using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Startup.Contracts
{
    public interface IApp
    {
        string[] Args { get; }
        IServiceProvider ApplicationServices { get; }
    }
}
