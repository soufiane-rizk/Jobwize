using JobWize.Shared.Runtime.Execution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JobWize.Shared.Application.Modules
{
    public interface IModule
    {
        ModuleRuntime Initialize(IServiceCollection services, IConfiguration configuration);
    }
}
