using JobWize.Modules.Identity;
using JobWize.Shared;
using JobWize.Shared.Application.Modules;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Runtime.Execution;

namespace JobWize.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            IServiceCollection services = builder.Services;
            IConfiguration configuration = builder.Configuration;

            services.AddShared();
            services.AddApi();

            List<IModule> modules =
            [
                new IdentityModule(),

                // new ProfileModule(),
                // new JobsModule(),
            ];

            List<ModuleRuntime> runtimes = [];

            foreach (IModule module in modules)
            {
                runtimes.Add(module.Initialize(services, configuration));
            }

            services.AddSingleton<IModuleRegistry>(new ModuleRegistry(runtimes));

            WebApplication app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.MapEndpoints();

            app.Run();
        }
    }
}
