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

            Guid[] companyIds = applications
                .Where(application => application.CompanyId is not null)
                .Select(application => application.CompanyId!.Value)
                .Distinct()
                .ToArray();

            Dictionary<Guid, string> companyNames = await dbContext.CompanyProjections
                .AsNoTracking()
                .Where(company => companyIds.Contains(company.Id))
                .ToDictionaryAsync(company => company.Id, company => company.Name, cancellationToken);

            Guid[] companyLocationIds = applications
                .Where(application => application.CompanyLocationId is not null)
                .Select(application => application.CompanyLocationId!.Value)
                .Distinct()
                .ToArray();

            Dictionary<Guid, string> companyLocationLabels = await dbContext.CompanyLocationProjections
                .AsNoTracking()
                .Where(location => companyLocationIds.Contains(location.Id))
                .ToDictionaryAsync(location => location.Id, location => location.Label, cancellationToken);

            var items = applications
                .Select(application =>
                {
                    JobInterview? lastInterview = application.Interviews
                        .OrderByDescending(interview => interview.ScheduledAt)
                        .FirstOrDefault();

                    return new Contracts.Public.JobApplications.GetJobApplications.Item(
                        application.Id,
                        GetCompanyName(application, companyNames),
                        GetCompanyLocationLabel(application, companyLocationLabels),
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

        private static string GetCompanyName(
            JobApplication application,
            IReadOnlyDictionary<Guid, string> companyNames)
        {
            if (application.CompanyId is Guid companyId && companyNames.TryGetValue(companyId, out string? companyName))
            {
                return companyName;
            }

            return application.LegacyCompanyName ?? "Unknown company";
        }

        private static string? GetCompanyLocationLabel(
            JobApplication application,
            IReadOnlyDictionary<Guid, string> companyLocationLabels)
        {
            if (application.CompanyLocationId is Guid companyLocationId &&
                companyLocationLabels.TryGetValue(companyLocationId, out string? companyLocationLabel))
            {
                return companyLocationLabel;
            }

            return null;
        }
    }
}
