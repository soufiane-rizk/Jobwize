using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using JobWize.Modules.Applications.Contracts.Public.Interviews;
using JobWize.Modules.Applications.Domain;
using JobWize.Modules.Applications.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Applications.Application.JobApplications;
public static class GetJobApplications
{
    internal sealed record Query : IQuery<Contracts.Public.JobApplications.GetJobApplications.Response>;

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    Contracts.Public.JobApplications.GetJobApplications.Route,
                    async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
                    {
                        Result<Contracts.Public.JobApplications.GetJobApplications.Response> result =
                            await dispatcher.SendAsync(new Query(), cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("GetJobApplications")
                .WithTags("Job applications");
        }
    }

    internal sealed class Handler(
        ApplicationsDbContext dbContext,
        IUserContext userContext) : IQueryHandler<Query, Contracts.Public.JobApplications.GetJobApplications.Response>
    {
        public async Task<Result<Contracts.Public.JobApplications.GetJobApplications.Response>> HandleAsync(Query query, CancellationToken cancellationToken)
        {
            var applications = await dbContext.JobApplications
                .AsNoTracking()
                .Where(application => application.CandidateId == userContext.UserId)
                .OrderByDescending(application => application.AppliedOn)
                .ThenByDescending(application => application.CreatedAt)
                .Include(application => application.Interviews)
                .ToListAsync(cancellationToken);

            var items = applications
                .Select(application =>
                {
                    JobInterview? lastInterview = application.Interviews
                        .OrderByDescending(interview => interview.ScheduledAt)
                        .FirstOrDefault();

                    return new Contracts.Public.JobApplications.GetJobApplications.Item(
                    application.Id,
                    application.CompanyName,
                    application.RoleTitle,
                    application.Kind,
                    application.Status,
                    application.LastActivityAt,
                    lastInterview?.Id,
                    lastInterview?.State,
                    lastInterview?.ScheduledAt,
                    application.AllowedNextStatuses);
                })
                .ToList();

            return Result<Contracts.Public.JobApplications.GetJobApplications.Response>.Success(new(items));
        }
    }
}
