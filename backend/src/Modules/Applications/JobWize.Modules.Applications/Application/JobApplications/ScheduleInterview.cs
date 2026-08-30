using FluentValidation;
using JobWize.Modules.Applications.Contracts.Events.JobApplications;
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
using ScheduleInterviewContract = JobWize.Modules.Applications.Contracts.Public.Interviews.ScheduleInterview;

namespace JobWize.Modules.Applications.Application.JobApplications;

public static class ScheduleInterview
{
    internal sealed record Command(
        Guid ApplicationId,
        InterviewType Type,
        DateTime ScheduledAt,
        int? DurationMinutes,
        InterviewFormat Format,
        string? Location,
        string? PreparationNotes,
        IReadOnlyList<ScheduleInterviewContract.Participant> Participants)
        : ICommand<ScheduleInterviewContract.Response>;

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.ScheduledAt)
                .NotEqual(default(DateTime));

            RuleFor(command => command.DurationMinutes)
                .GreaterThan(0)
                .When(command => command.DurationMinutes.HasValue);

            RuleForEach(command => command.Participants)
                .ChildRules(participant =>
                {
                    participant.RuleFor(item => item.Name)
                        .NotEmpty()
                        .MaximumLength(200);

                    participant.RuleFor(item => item.RoleTitle)
                        .MaximumLength(200);
                });
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    ScheduleInterviewContract.Route,
                    async (
                        Guid id,
                        ScheduleInterviewContract.Request request,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<ScheduleInterviewContract.Response> result = await dispatcher.SendAsync(
                            new Command(
                                id,
                                request.Type,
                                request.ScheduledAt,
                                request.DurationMinutes,
                                request.Format,
                                request.Location,
                                request.PreparationNotes,
                                request.Participants),
                            cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("ScheduleInterview")
                .WithTags("Interviews");
        }
    }

    internal sealed class Handler(
        IJobApplicationRepository applications,
        IUserContext currentUser,
        IDispatcher dispatcher)
        : ICommandHandler<Command, ScheduleInterviewContract.Response>
    {
        public async Task<Result<ScheduleInterviewContract.Response>> HandleAsync(
            Command command,
            CancellationToken cancellationToken)
        {
            JobApplication? application = await applications.GetByIdAsync(
                command.ApplicationId,
                currentUser.UserId,
                cancellationToken);

            if (application is null)
            {
                return Result<ScheduleInterviewContract.Response>.Failure(
                    ApplicationsErrors.JobApplicationNotFound);
            }

            if (application.Status is ApplicationStatus.Draft or ApplicationStatus.Planned)
            {
                return Result<ScheduleInterviewContract.Response>.Failure(
                    ApplicationsErrors.ApplicationMustBeSentBeforeInterview);
            }

            if (application.Status is not (ApplicationStatus.Applied or ApplicationStatus.InProcess))
            {
                return Result<ScheduleInterviewContract.Response>.Failure(
                    ApplicationsErrors.CannotScheduleInterviewForCurrentStatus);
            }

            if (application.Status == ApplicationStatus.Applied)
            {
                application.ChangeStatus(
                    ApplicationStatus.InProcess,
                    null,
                    "Interview process started.");
            }

            JobInterview interview = application.ScheduleInterview(
                command.Type,
                command.ScheduledAt,
                command.DurationMinutes,
                command.Format,
                command.Location,
                command.PreparationNotes,
                command.Participants.Select(participant =>
                    (participant.Name, participant.RoleTitle)));

            await applications.SaveAsync(application, cancellationToken);

            await dispatcher.PublishAsync(
                new JobInterviewScheduled(interview.Id, application.Id, currentUser.UserId),
                cancellationToken);

            return Result<ScheduleInterviewContract.Response>.Success(new(interview.Id));
        }
    }
}
