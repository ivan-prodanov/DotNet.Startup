using DotNet.Startup.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DotNet.Startup.Contracts
{
    public interface IStartup
    {
        void ConfigureServices(IServiceCollection services);
        void Run(IApp app, IAppEnvironment env);

        IConfigurationRoot Configuration { get; set; }
    }
}
