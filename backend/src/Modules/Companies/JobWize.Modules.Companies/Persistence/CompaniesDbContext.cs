using JobWize.Modules.Companies.Domain;
using JobWize.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Companies.Persistence;

public sealed class CompaniesDbContext(DbContextOptions<CompaniesDbContext> options) : ModuleDbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyLocation> CompanyLocations => Set<CompanyLocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CompaniesDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
