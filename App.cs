using DotNet.Startup.Contracts;
using System;

namespace DotNet.Startup
{
    public class App : IApp
    {
        public string[] Args { get; set; }

        public IServiceProvider ApplicationServices { get; set; }
    }
}
