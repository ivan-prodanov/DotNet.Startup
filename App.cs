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
    public class App : IApp
    {
        public string[] Args { get; set; }

        public IServiceProvider ApplicationServices { get; set; }
    }
}
