using JobWize.Runtime.Contracts.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobWize.Modules.Files.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddFilesPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Files")
            ?? throw new InvalidOperationException("Connection string 'Files' was not found.");

        services.AddDbContext<FilesDbContext>(options => options.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.MigrationsAssembly(typeof(FilesDbContext).Assembly.FullName);
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "files");
        }));

        services.AddScoped<ITransactionContext>(provider => provider.GetRequiredService<FilesDbContext>());
        services.AddScoped<IFileAssetRepository, FileAssetRepository>();
        return services;
    }
}
