using JobWize.Modules.Identity;
using JobWize.Runtime.Contracts.DependencyInjection;
using JobWize.Runtime.Contracts.Modules;
using JobWize.Runtime.DependencyInjection;
using JobWize.Runtime.Execution;
using JobWize.Runtime.Registration;
using JobWize.Shared;
using JobWize.Shared.Endpoints;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

namespace JobWize.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            IServiceCollection services = builder.Services;
            IConfiguration configuration = builder.Configuration;

            services.AddRuntime(
                configuration,
                options =>
                {
                    options
                        .AddModule(new IdentityModule());
                    // options.AddModule(new ProfileModule());

                    // options.AddPipeline<ValidationBehavior<,>>();
                    // options.AddPipeline<TransactionBehavior<,>>();
                });

            services.AddShared();
            services.AddApi(configuration);

            WebApplication app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapApi();

            app.UseHttpsRedirection();

            app.MapEndpoints();

            app.Run();
        }
    }
}
