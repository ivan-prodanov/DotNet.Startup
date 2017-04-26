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
    public class AppLoader : IAppLoader
    {
        private App _app;
        private IAppEnvironment _environment;

        public AppLoader(string name, IEnumerable<string> tags, string[] _commandlineArguments)
        {
            _app = new App
            {
                Args = _commandlineArguments,
            };
            _environment = new AppEnvironment(name, tags);
        }

        private object CreateInstance(Type startupType)
        {
            try
            {
                return Activator.CreateInstance(startupType, _environment);
            }
            catch (MissingMethodException)
            {
                return Activator.CreateInstance(startupType);
            }
        }

        private Type GetStartupType()
        {
            bool IsAConventionBasedStartupClass(Type t)
            {
                return t.Name == "Startup"
                    && !t.GetTypeInfo().IsAbstract
                    && MethodLoader.TryGetMethodInfo<Action<IServiceCollection>>(t, nameof(IStartup.ConfigureServices), out var @delegate);
            }
            
            var startupTypes = Assembly.GetEntryAssembly()
                    .GetTypes()
                    .Where(t => t.GetTypeInfo().BaseType == typeof(IStartup) || IsAConventionBasedStartupClass(t))
                    .ToList();

            if (!startupTypes.Any())
            {
                throw new InvalidOperationException("There is more than one Startup class.");
            }

            var startupType = startupTypes
                .Where(st => st.Name == $"Startup.{_environment.EnvironmentName}")
                .FirstOrDefault() ??
            startupTypes
                .Where(st => st.Name == "Startup")
                .First();

            return startupType;
        }

        private (IStartup startup,  IServiceProvider provider) GetConventionalStartup(object instance, IServiceCollection serviceCollection)
        {
            var configureServicesMethod = MethodLoader.GetMethod(instance, nameof(IStartup.ConfigureServices), serviceCollection);

            var startup = new ConventionBasedStartup(instance)
            {
                ConfigurationCallBack = configureServicesMethod,
                //Configuration = _configurationBuilder.Build()
            };

            startup.ConfigureServices(serviceCollection);

            var provider = serviceCollection.BuildServiceProvider();
            var runMethod = MethodLoader.GetMethod(instance, nameof(IStartup.Run), _app, _environment);
            startup.RunCallback = runMethod;

            return (startup, provider);
        }

        private (IStartup startup, IServiceProvider provider) SetupStartup(object instance, Type startupType, IServiceCollection serviceCollection)
        {
            if (!typeof(IStartup).GetTypeInfo().IsAssignableFrom(startupType))
            {
                var conventionalStartup = GetConventionalStartup(instance, serviceCollection);
                return conventionalStartup;
            }

            var startup = instance as IStartup;
            startup.ConfigureServices(serviceCollection);

            return (startup, serviceCollection.BuildServiceProvider());
        }

        public void Run()
        {
            var startupType = GetStartupType();
            
            var serviceCollection = new ServiceCollection();
            object instance = CreateInstance(startupType);

            var startupInfo = SetupStartup(instance, startupType, serviceCollection);

            _app.ApplicationServices = startupInfo.provider;

            startupInfo.startup.Run(_app, _environment);
        }
    }
}
