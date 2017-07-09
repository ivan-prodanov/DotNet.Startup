using DotNet.Startup.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        private Type GetStartupType(Assembly assembly)
        {
            bool IsAConventionBasedStartupClass(Type t)
            {
                return t.Name.StartsWith("Startup")
                    && !t.GetTypeInfo().IsAbstract
                    && MethodLoader.TryGetMethodInfo<IServiceCollection>(t, nameof(IStartup.ConfigureServices), out var @delegate);
            }

            var startupTypes = assembly?
                    .GetTypes()
                    .Where(t => t.GetTypeInfo().BaseType == typeof(IStartup) || IsAConventionBasedStartupClass(t))
                    .ToList();

            if (!startupTypes.Any())
            {
                throw new InvalidOperationException("There is no Startup class");
            }

            var startupType = startupTypes
                .Where(st => st.Name == $"Startup_{_environment.EnvironmentName}")
                .FirstOrDefault() ??
            startupTypes
                .Where(st => st.Name == "Startup")
                .First();

            return startupType;
        }

        private Func<IServiceCollection> GetConfigureServicesdDelegateAsFunc(object instance, IServiceCollection serviceCollection)
        {
            if (MethodLoader.TryGetMethodInfo<IServiceCollection>(instance.GetType(), $"Configure{_environment.EnvironmentName}Services", out var configureServicesMethod))
            {
                return MethodLoader.GetMethod<IServiceCollection>(instance, $"Configure{_environment.EnvironmentName}Services", serviceCollection);
            }
            else
            {
                return MethodLoader.GetMethod<IServiceCollection>(instance, nameof(IStartup.ConfigureServices), serviceCollection);
            }
        }
        private Action GetConfigureServicesdDelegateAsAction(object instance, IServiceCollection serviceCollection)
        {
            if (MethodLoader.TryGetMethodInfo<IServiceCollection>(instance.GetType(), $"Configure{_environment.EnvironmentName}Services", out var configureServicesMethod))
            {
                return MethodLoader.GetMethod(instance, $"Configure{_environment.EnvironmentName}Services", serviceCollection);
            }
            else
            {
                return MethodLoader.GetMethod(instance, nameof(IStartup.ConfigureServices), serviceCollection);
            }
        }

        private Action GetConfigureServicesdDelegate(object instance, IServiceCollection serviceCollection)
        {
            var funcDelegate = GetConfigureServicesdDelegateAsFunc(instance, serviceCollection);
            if (funcDelegate != null)
            {
                return () => { funcDelegate(); };
            }

            return GetConfigureServicesdDelegateAsAction(instance, serviceCollection);
        }

        private (IStartup startup, IServiceProvider provider) GetConventionalStartup(object instance, IServiceCollection serviceCollection)
        {
            var configureServicesMethod = GetConfigureServicesdDelegate(instance, serviceCollection);

            var startup = new ConventionBasedStartup(instance)
            {
                ConfigurationCallBack = configureServicesMethod
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

        public void Run(Assembly assembly)
        {
            var startupType = GetStartupType(assembly);

            var serviceCollection = new ServiceCollection();
            object instance = CreateInstance(startupType);

            var startupInfo = SetupStartup(instance, startupType, serviceCollection);

            _app.ApplicationServices = startupInfo.provider;

            startupInfo.startup.Run(_app, _environment);
        }

        public void Run()
        {
            Run(Assembly.GetEntryAssembly());
        }
    }
}
