using JobWize.Shared.Contracts.Application.Dispatching;
using JobWize.Shared.Runtime.Discovery;
using JobWize.Shared.Runtime.Execution;
using JobWize.Shared.Runtime.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
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

        public ModuleRuntime Initialize(IServiceCollection services, IConfiguration configuration)
        {
            Configure(services, configuration);

            return BuildRuntime(services);
        }

        protected virtual ModuleRuntime BuildRuntime(IServiceCollection services)
        {
            HandlerScanner scanner = new();

            ModuleDescriptor descriptor = scanner.Scan(Assembly);

            ModuleValidator.Validate(descriptor);

            RegisterHandlers(services, descriptor);

            return new ModuleRuntime(Name, descriptor);
        }

        protected abstract void Configure(IServiceCollection services, IConfiguration configuration);

        protected virtual void RegisterHandlers(IServiceCollection services, ModuleDescriptor descriptor)
        {
            foreach (HandlerDescriptor handler in descriptor.Handlers)
            {
                services.AddScoped(handler.HandlerType);
            }

            foreach (HandlerDescriptor handler in descriptor.NotificationHandlers)
            {
                services.AddScoped(handler.HandlerType);
            }
        }
    }
}
