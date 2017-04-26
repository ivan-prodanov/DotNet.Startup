using DotNet.Startup.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Startup
{
    class AppEnvironment : IAppEnvironment
    {
        public string EnvironmentName { get; }

        public IReadOnlyList<string> EnvironmentTags { get; }

        public AppEnvironment(string environment, IEnumerable<string> tags)
        {
            EnvironmentName = environment;
            EnvironmentTags = new List<string>(tags);
        }
        public AppEnvironment(string environment) : this(environment, new List<string>()) { }
    }
}
