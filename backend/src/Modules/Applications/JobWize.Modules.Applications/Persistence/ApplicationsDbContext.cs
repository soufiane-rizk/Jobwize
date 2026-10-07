using JobWize.Modules.Applications.Domain;
using JobWize.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Applications.Persistence;

public sealed class ApplicationsDbContext : ModuleDbContext
{
    public ApplicationsDbContext(DbContextOptions<ApplicationsDbContext> options)
        : base(options)
    {
    }

    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<JobInterview> JobInterviews => Set<JobInterview>();
    public DbSet<CompanyProjection> CompanyProjections => Set<CompanyProjection>();
    public DbSet<CompanyLocationProjection> CompanyLocationProjections => Set<CompanyLocationProjection>();
    public DbSet<CompanyContactProjection> CompanyContactProjections => Set<CompanyContactProjection>();
    public DbSet<JobApplicationReminder> JobApplicationReminders => Set<JobApplicationReminder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationsDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
