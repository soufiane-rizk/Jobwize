using JobWize.Runtime.Contracts.DependencyInjection;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Contracts.Modules;
using JobWize.Runtime.Dispatching;
using JobWize.Runtime.Execution;
using JobWize.Runtime.Registration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.DependencyInjection
{
    public static class RuntimeServiceCollectionExtensions
    {
        public static IServiceCollection AddRuntime(this IServiceCollection services, IConfiguration configuration, Action<RuntimeOptions> configure)
        {
            RuntimeOptions options = new();

            configure(options);

            AddDispatching(services);

            RuntimeBuilder runtimeBuilder = new();

            List<ModuleRuntime> runtimes = [];

            foreach (IModule module in options.Modules)
            {
                module.ConfigureServices(services, configuration);

                runtimes.Add(runtimeBuilder.Build(module, services));
            }

            services.AddSingleton<IModuleRegistry>(new ModuleRegistry(runtimes));

            services.AddSingleton(options);

            return services;
        }

        private static void AddDispatching(IServiceCollection services)
        {
            services.AddScoped<IDispatcher, Dispatcher>();

            services.AddScoped<INotificationContext, NotificationContext>();

            services.AddScoped<IExecutionModel, MonolithExecutionModel>();
        }

    }
}
