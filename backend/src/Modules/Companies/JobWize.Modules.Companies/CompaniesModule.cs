using JobWize.Modules.Companies.Persistence;
using JobWize.Runtime.Registration;
using JobWize.Shared.Endpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace JobWize.Modules.Companies;

public sealed class CompaniesModule : ModuleBase
{
    public override string Name => "Companies";

    protected override void Configure(IServiceCollection services, IConfiguration configuration)
    {
        services.AddCompaniesPersistence(configuration);
        services.AddEndpoints(Assembly);
    }
}
