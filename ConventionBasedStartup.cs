using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using DotNet.Startup.Contracts;

namespace DotNet.Startup
{
    internal class ConventionBasedStartup : IStartup
    {
        public Action ConfigurationCallBack { get; set; }
        public Action RunCallback { get; set; }

        private readonly object _instance;

        public ConventionBasedStartup(object instance)
        {
            _instance = instance;
        }

        public IConfigurationRoot Configuration
        {
            get => _instance.GetType().GetTypeInfo().GetProperty(nameof(Configuration))?.GetValue(_instance) as IConfigurationRoot;
            set
            {
                _instance.GetType().GetTypeInfo().GetProperty(nameof(Configuration))?.SetValue(_instance, value);
            }
        }

        public void ConfigureServices(IServiceCollection _)
        {
            ConfigurationCallBack();
        }

        public void Run(IApp app, IAppEnvironment env)
        {
            RunCallback();
        }
    }
}
