using JobWize.Modules.Applications.Contracts.Public.JobApplications;
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

public static class GetJobApplication
{
    internal sealed record Query(Guid Id) : IQuery<Contracts.Public.JobApplications.GetJobApplication.Response>;

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    Contracts.Public.JobApplications.GetJobApplication.Route,
                    async (
                        Guid id,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<Contracts.Public.JobApplications.GetJobApplication.Response> result =
                            await dispatcher.SendAsync(new Query(id), cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("GetJobApplication")
                .WithTags("Job applications");
        }
    }

    internal sealed class Handler(
        ApplicationsDbContext dbContext,
        IUserContext userContext) : IQueryHandler<Query, Contracts.Public.JobApplications.GetJobApplication.Response>
    {
        public async Task<Result<Contracts.Public.JobApplications.GetJobApplication.Response>> HandleAsync(
            Query query,
            CancellationToken cancellationToken)
        {
            var application = await dbContext.JobApplications
                .AsNoTracking()
                .Include(item => item.Activities)
                .Include(item => item.Interviews)
                .ThenInclude(interview => interview.Participants)
                .Include(item => item.CvSubmissions)
                .ThenInclude(submission => submission.Documents)
                .SingleOrDefaultAsync(
                    item => item.Id == query.Id && item.CandidateId == userContext.UserId,
                    cancellationToken);

            if (application is null)
            {
                return Result<Contracts.Public.JobApplications.GetJobApplication.Response>.Failure(
                    ApplicationsErrors.JobApplicationNotFound);
            }

            string companyName = string.IsNullOrWhiteSpace(application.LegacyCompanyName)
                ? "Unknown company"
                : application.LegacyCompanyName;
            string? companyLocationLabel = null;

            if (application.CompanyId is Guid companyId)
            {
                companyName = await dbContext.CompanyProjections
                    .AsNoTracking()
                    .Where(company => company.Id == companyId)
                    .Select(company => company.Name)
                    .SingleOrDefaultAsync(cancellationToken)
                    ?? companyName;
            }

            if (application.CompanyLocationId is Guid companyLocationId)
            {
                companyLocationLabel = await dbContext.CompanyLocationProjections
                    .AsNoTracking()
                    .Where(location => location.Id == companyLocationId)
                    .Select(location => location.Label)
                    .SingleOrDefaultAsync(cancellationToken);
            }

            var activities = application.Activities
                .OrderByDescending(activity => activity.OccurredAt)
                .Select(activity => new Contracts.Public.JobApplications.GetJobApplication.ActivityItem(
                    activity.Id, activity.Type, activity.Status, activity.OccurredAt, activity.Note))
                .ToList();

            var interviews = application.Interviews
                .OrderBy(interview => interview.ScheduledAt)
                .Select(interview => new Contracts.Public.JobApplications.GetJobApplication.InterviewItem(
                    interview.Id,
                    interview.Type,
                    interview.State,
                    interview.ScheduledAt,
                    interview.DurationMinutes,
                    interview.Format,
                    interview.Location,
                    interview.PreparationNotes,
                    interview.Participants
                        .Select(participant => new Contracts.Public.JobApplications.GetJobApplication.InterviewParticipantItem(
                            participant.Id,
                            participant.Name,
                            participant.RoleTitle))
                        .ToList()))
                .ToList();

            var submissions = application.CvSubmissions
                .OrderByDescending(item => item.SentAt)
                .Select(item => new Contracts.Public.JobApplications.GetJobApplication.CvSubmissionItem(
                    item.Id,
                    item.SentAt,
                    item.Method,
                    item.Notes,
                    item.CompanyContactId,
                    item.CompanyLocationId,
                    item.ContactName,
                    item.ContactRoleTitle,
                    item.ContactEmail,
                    item.ContactPhoneNumber,
                    item.Documents
                        .Select(document => new Contracts.Public.JobApplications.GetJobApplication.CvSubmissionDocumentItem(
                            document.FileId,
                            document.FileName,
                            document.ContentType,
                            document.SizeBytes))
                        .ToList()))
                .ToList();

            return Result<Contracts.Public.JobApplications.GetJobApplication.Response>.Success(new(
                application.Id,
                application.CompanyId,
                companyName,
                companyLocationLabel,
                application.RoleTitle,
                application.Kind,
                application.Status,
                application.AppliedOn,
                application.SourceUrl,
                application.Notes,
                activities,
                interviews,
                submissions,
                application.AllowedNextStatuses));
        }
    }
}
