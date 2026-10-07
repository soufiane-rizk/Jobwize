using FluentValidation;
using JobWize.Modules.Applications.Contracts.Public.Reminders;
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

public static class GetAgenda
{
    internal sealed record Query(DateTime From, DateTime To)
        : IQuery<Contracts.Public.Reminders.GetAgenda.Response>;

    internal sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(query => query.From).NotEqual(default(DateTime));
            RuleFor(query => query.To).NotEqual(default(DateTime));
            RuleFor(query => query)
                .Must(query => query.To > query.From)
                .WithMessage("The agenda end must be after its start.");
            RuleFor(query => query)
                .Must(query => query.To - query.From <= TimeSpan.FromDays(366))
                .WithMessage("The agenda range cannot exceed 366 days.");
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    Contracts.Public.Reminders.GetAgenda.Route,
                    async (
                        DateTime from,
                        DateTime to,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<Contracts.Public.Reminders.GetAgenda.Response> result =
                            await dispatcher.SendAsync(
                                new Query(from, to),
                                cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("GetAgenda")
                .WithTags("Reminders");
        }
    }

    internal sealed class Handler(ApplicationsDbContext dbContext, IUserContext user)
        : IQueryHandler<Query, Contracts.Public.Reminders.GetAgenda.Response>
    {
        public async Task<Result<Contracts.Public.Reminders.GetAgenda.Response>> HandleAsync(
            Query query,
            CancellationToken cancellationToken)
        {
            DateTime from = ToUtc(query.From);
            DateTime to = ToUtc(query.To);

            List<Domain.JobApplication> applications = await dbContext.JobApplications
                .AsNoTracking()
                .Where(application => application.CandidateId == user.UserId)
                .Include(application => application.Interviews.Where(interview =>
                    interview.ScheduledAt >= from &&
                    interview.ScheduledAt < to))
                .Include(application => application.Reminders.Where(reminder =>
                    reminder.DueAt >= from &&
                    reminder.DueAt < to &&
                    reminder.State == ReminderState.Open))
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

            List<Contracts.Public.Reminders.GetAgenda.Item> items = applications
                .SelectMany(application => CreateItems(application, companyNames, from, to))
                .OrderBy(item => item.OccursAt)
                .ToList();

            return Result<Contracts.Public.Reminders.GetAgenda.Response>.Success(new(items));
        }

        private static IEnumerable<Contracts.Public.Reminders.GetAgenda.Item> CreateItems(
            Domain.JobApplication application,
            IReadOnlyDictionary<Guid, string> companyNames,
            DateTime from,
            DateTime to)
        {
            string companyName = application.CompanyId is Guid companyId &&
                                 companyNames.TryGetValue(companyId, out string? projectedName)
                ? projectedName
                : application.LegacyCompanyName ?? "Unknown company";

            IEnumerable<Contracts.Public.Reminders.GetAgenda.Item> interviews = application.Interviews
                .Where(interview => interview.ScheduledAt >= from && interview.ScheduledAt < to)
                .Select(interview => new Contracts.Public.Reminders.GetAgenda.Item(
                    interview.Id,
                    application.Id,
                    companyName,
                    application.RoleTitle,
                    interview.ScheduledAt,
                    $"{interview.Type} interview",
                    interview.Location,
                    null,
                    null,
                    interview.State));

            IEnumerable<Contracts.Public.Reminders.GetAgenda.Item> reminders = application.Reminders
                .Where(reminder =>
                    reminder.DueAt >= from &&
                    reminder.DueAt < to &&
                    reminder.State == ReminderState.Open)
                .Select(reminder => new Contracts.Public.Reminders.GetAgenda.Item(
                    reminder.Id,
                    application.Id,
                    companyName,
                    application.RoleTitle,
                    reminder.DueAt,
                    reminder.Title,
                    reminder.Note,
                    reminder.Kind,
                    reminder.State,
                    null));

            return interviews.Concat(reminders);
        }

        private static DateTime ToUtc(DateTime value) =>
            value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
    }
}
