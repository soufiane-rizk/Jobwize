using JobWize.Runtime.Contracts.DependencyInjection;
using JobWize.Runtime.Discovery;
using JobWize.Runtime.Execution;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Registration
{
    internal sealed class RuntimeBuilder
    {
        public ModuleRuntime Build(IModule module, IServiceCollection services)
        {
            HandlerScanner scanner = new();

            ModuleDescriptor descriptor = scanner.Scan(module.Assembly);

            ModuleValidator.Validate(descriptor);

            RegisterHandlers(services, descriptor);

            return new ModuleRuntime(module.Name, descriptor);
        }

        private static void RegisterHandlers(IServiceCollection services, ModuleDescriptor descriptor)
        {
            foreach (HandlerDescriptor handler in descriptor.Handlers)
            {
                services.AddScoped(handler.HandlerType);
            }

            foreach (HandlerDescriptor handler in descriptor.ModuleQueryHandlers)
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
