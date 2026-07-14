using JobWize.Shared.Infrastructure.Dispatching;
using JobWize.Shared.Infrastructure.Time;
using JobWize.Shared.Runtime.Dispatching;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddShared(this IServiceCollection services)
        {
            services.AddDispatching();

            services.AddTime();

            return services;
        }

        private static IServiceCollection AddDispatching(this IServiceCollection services)
        {
            services.AddScoped<IDispatcher, Dispatcher>();
            services.AddScoped<IModuleDispatcher, InProcessModuleDispatcher>();
            services.AddScoped<IIntegrationEventContext, IntegrationEventContext>();

            return services;
        }

        private static IServiceCollection AddTime(this IServiceCollection services)
        {
            services.AddSingleton<IClock, UtcClock>();

            return services;
        }
    }
}
