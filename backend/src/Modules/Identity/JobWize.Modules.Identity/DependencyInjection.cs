using JobWize.Modules.Identity.Infrastructure;
using JobWize.Modules.Identity.Persistence;
using JobWize.Shared.Endpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Modules.Identity
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddPersistence(configuration);

            services.AddInfrastructure(configuration);

            services.AddEndpoints(typeof(DependencyInjection).Assembly);

            return services;
        }
    }
}
