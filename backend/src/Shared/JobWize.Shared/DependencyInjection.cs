using JobWize.Shared.Infrastructure.Time;
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
            services.AddTime();

            return services;
        }

        private static IServiceCollection AddTime(this IServiceCollection services)
        {
            services.AddSingleton<IClock, UtcClock>();

            return services;
        }
    }
}
