using JobWize.Runtime.Contracts.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobWize.Modules.Companies.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddCompaniesPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Companies")
            ?? throw new InvalidOperationException("Connection string 'Companies' was not found.");

        services.AddDbContext<CompaniesDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(CompaniesDbContext).Assembly.FullName);
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "companies");
            });
        });

        services.AddScoped<ITransactionContext>(provider => provider.GetRequiredService<CompaniesDbContext>());
        services.AddScoped<ICompanyRepository, CompanyRepository>();

        return services;
    }
}
