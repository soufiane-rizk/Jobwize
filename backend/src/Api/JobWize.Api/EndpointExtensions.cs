using JobWize.Api.Health;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace JobWize.Api
{
    public static class EndpointExtensions
    {
        public static WebApplication MapApi(this WebApplication app)
        {
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = HealthResponseWriter.WriteResponse
            });

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = HealthResponseWriter.WriteResponse
            });

            return app;
        }
    }
}
