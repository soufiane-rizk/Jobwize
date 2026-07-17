using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JobWize.Runtime.Contracts.DependencyInjection
{
    public interface IModule
    {
        string Name { get; }

        Assembly Assembly { get; }

        void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    }
}
