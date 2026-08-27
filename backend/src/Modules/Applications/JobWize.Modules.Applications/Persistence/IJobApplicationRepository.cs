using JobWize.Modules.Applications.Domain;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Applications.Persistence;

public interface IJobApplicationRepository
{
    Task<JobApplication?> GetByIdAsync(
        Guid applicationId,
        Guid candidateId,
        CancellationToken cancellationToken = default);

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

    public Task<JobApplication?> GetByIdAsync(
        Guid applicationId,
        Guid candidateId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.JobApplications
            .Include(application => application.Activities)
            .Include(application => application.Interviews)
            .ThenInclude(interview => interview.Participants)
            .SingleOrDefaultAsync(
                application => application.Id == applicationId && application.CandidateId == candidateId,
                cancellationToken);
    }
}
