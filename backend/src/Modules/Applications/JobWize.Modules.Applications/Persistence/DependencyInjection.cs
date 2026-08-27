using JobWize.Runtime.Contracts.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobWize.Modules.Applications.Persistence;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationsPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Applications")
            ?? throw new InvalidOperationException(
                "Connection string 'Applications' was not found.");

        services.AddDbContext<ApplicationsDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(ApplicationsDbContext).Assembly.FullName);
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "applications");
            });
        });

        services.AddScoped<ITransactionContext>(provider => provider.GetRequiredService<ApplicationsDbContext>());
        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();

        return services;
    }
}
