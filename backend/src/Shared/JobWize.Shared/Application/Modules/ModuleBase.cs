using JobWize.Runtime.Contracts.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JobWize.Shared.Application.Modules
{
    public abstract class ModuleBase : IModule
    {
        public abstract string Name { get; }

        public virtual Assembly Assembly => GetType().Assembly;

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            Configure(services, configuration);
        }

        protected abstract void Configure(IServiceCollection services, IConfiguration configuration);
    }
}
