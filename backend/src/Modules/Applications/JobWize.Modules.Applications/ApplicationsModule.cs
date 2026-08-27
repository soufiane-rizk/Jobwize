using JobWize.Modules.Applications.Persistence;
using JobWize.Runtime.Registration;
using JobWize.Shared.Endpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace JobWize.Modules.Applications;

public sealed class ApplicationsModule : ModuleBase
{
    public override string Name => "Applications";
    protected override void Configure(IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationsPersistence(configuration);
        services.AddEndpoints(Assembly);
    }
}
