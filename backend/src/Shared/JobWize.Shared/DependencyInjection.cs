using JobWize.Shared.Application.Security;
using JobWize.Shared.Infrastructure.Security;
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

            services.AddHttpContextAccessor();
            services.AddScoped<IUserContext, HttpUserContext>();

            return services;
        }

        private static IServiceCollection AddTime(this IServiceCollection services)
        {
            services.AddSingleton<IClock, UtcClock>();

            return services;
        }
    }
}
