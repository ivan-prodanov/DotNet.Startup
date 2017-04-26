using DotNet.Startup.Contracts;
using DevOps.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotNet.Startup
{
    public class AppBuilder
    {
        Func<(string EnvironmentName, IEnumerable<string> EnvironmentTags)> _getEnvironmentName;
        string[] _args = new string[0];

        public AppBuilder() 
            => UseConventionalEnvironments();

        public AppBuilder UseCustomEnvironments(Func<(string, IEnumerable<string>)> getEnvironmentName)
        {
            _getEnvironmentName = getEnvironmentName;

            return this;
        }

        public AppBuilder UseCommandLine(string[] args)
        {
            _args = args;

            return this;
        }

        public AppBuilder UseConventionalEnvironments()
        {
            _getEnvironmentName = () =>
            {
                var environmentVariable = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
                if(environmentVariable == "Development" || environmentVariable == "Staging")
                {
                    return (environmentVariable, new List<string>());
                }

                return ("Production", new List<string>());
            };

            return this;
        }

        public IAppLoader Build()
        {
            var (name, tags) = _getEnvironmentName();
            return new AppLoader(name, tags, _args);
        }
    }
}
