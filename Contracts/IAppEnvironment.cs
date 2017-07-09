using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Startup.Contracts
{
    public interface IAppEnvironment
    {
        string EnvironmentName { get; }

        IReadOnlyList<string> EnvironmentTags { get; }
    }
}
