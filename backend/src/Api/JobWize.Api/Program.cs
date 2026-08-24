using FluentValidation;
using JobWize.Modules.Identity;
using JobWize.Runtime.Contracts.DependencyInjection;
using JobWize.Runtime.Contracts.Modules;
using JobWize.Runtime.DependencyInjection;
using JobWize.Runtime.Execution;
using JobWize.Runtime.Registration;
using JobWize.Shared;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Runtime.Behaviors;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Globalization;
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
                        .AddModule(new IdentityModule())

                        .AddPipeline(typeof(ExceptionHandlingBehavior<,>))
                        .AddPipeline(typeof(ValidationBehavior<,>))
                        .AddPipeline(typeof(TransactionBehavior<,>));
                });

            services.AddShared();
            services.AddApi(configuration);

            builder.Services.AddAuthorization();

            ValidatorOptions.Global.LanguageManager.Enabled = true;
            ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("en");

            string[] allowedOrigins =
                builder.Configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>()
                ?? [];

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            WebApplication app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("Frontend");

            app.MapApi();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapEndpoints();

            app.Run();
        }
    }
}
