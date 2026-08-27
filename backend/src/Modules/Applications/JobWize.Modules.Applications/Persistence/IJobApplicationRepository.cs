using JobWize.Modules.Applications.Domain;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Applications.Persistence;

public interface IJobApplicationRepository
{
    Task SaveAsync(
        JobApplication application,
        CancellationToken cancellationToken = default);
}

internal sealed class JobApplicationRepository : IJobApplicationRepository
{
    private readonly ApplicationsDbContext _dbContext;

    public JobApplicationRepository(ApplicationsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task SaveAsync(
        JobApplication application,
        CancellationToken cancellationToken = default)
    {
        if (_dbContext.Entry(application).State == EntityState.Detached)
        {
            _dbContext.JobApplications.Add(application);
        }

        return Task.CompletedTask;
    }
}
