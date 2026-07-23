using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JobWize.Shared.Endpoints
{
    public static class EndpointExtensions
    {
        public static IServiceCollection AddEndpoints(
            this IServiceCollection services,
            params Assembly[] assemblies)
        {
            var endpointTypes = assemblies
                .SelectMany(assembly => assembly.DefinedTypes)
                .Where(type =>
                    !type.IsAbstract &&
                    !type.IsInterface &&
                    typeof(IEndpoint).IsAssignableFrom(type));

            foreach (var endpointType in endpointTypes)
                services.AddTransient(typeof(IEndpoint), endpointType);

            return services;
        }

        public static IApplicationBuilder MapEndpoints(this WebApplication app)
        {
            var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

            foreach (var endpoint in endpoints)
                endpoint.MapEndpoint(app);

            return app;
        }
    }
}
