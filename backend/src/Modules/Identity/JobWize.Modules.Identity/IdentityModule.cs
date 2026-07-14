using JobWize.Modules.Identity.Infrastructure;
using JobWize.Modules.Identity.Persistence;
using JobWize.Shared.Application.Modules;
using JobWize.Shared.Endpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JobWize.Modules.Identity
{
    public sealed class IdentityModule : ModuleBase
    {
        public override string Name => "Identity";

        protected override void Configure(IServiceCollection services, IConfiguration configuration)
        {
            services.AddPersistence(configuration);

            services.AddInfrastructure(configuration);

            services.AddEndpoints(Assembly);
        }
    }
}
