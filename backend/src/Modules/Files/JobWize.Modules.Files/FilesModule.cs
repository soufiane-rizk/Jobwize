using JobWize.Modules.Files.Persistence;
using JobWize.Modules.Files.Storage;
using JobWize.Runtime.Registration;
using JobWize.Shared.Endpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace JobWize.Modules.Files;

public sealed class FilesModule : ModuleBase
{
    public override string Name => "Files";

    protected override void Configure(IServiceCollection services, IConfiguration configuration)
    {
        services.AddFilesPersistence(configuration);
        services.AddFileAssetStorage(configuration);
        services.AddEndpoints(Assembly);
    }
}
