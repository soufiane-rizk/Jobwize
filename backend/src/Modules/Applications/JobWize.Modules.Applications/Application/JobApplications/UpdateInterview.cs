using FluentValidation;
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
using UpdateInterviewContract = JobWize.Modules.Applications.Contracts.Public.Interviews.UpdateInterview;

namespace JobWize.Modules.Applications.Application.JobApplications;

public static class UpdateInterview
{
    internal sealed record Command(
        Guid ApplicationId,
        Guid InterviewId,
        InterviewType Type,
        DateTime ScheduledAt,
        int? DurationMinutes,
        InterviewFormat Format,
        string? Location,
        string? PreparationNotes,
        IReadOnlyList<UpdateInterviewContract.Participant> Participants) : ICommand<bool>;

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.ScheduledAt).NotEqual(default(DateTime));
            RuleFor(command => command.DurationMinutes)
                .GreaterThan(0)
                .When(command => command.DurationMinutes.HasValue);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    UpdateInterviewContract.Route,
                    async (Guid applicationId, Guid interviewId, UpdateInterviewContract.Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
                    {
                        Result<bool> result = await dispatcher.SendAsync(
                            new Command(applicationId, interviewId, request.Type, request.ScheduledAt, request.DurationMinutes, request.Format, request.Location, request.PreparationNotes, request.Participants),
                            cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("UpdateInterview")
                .WithTags("Interviews");
        }
    }

    internal sealed class Handler(IJobApplicationRepository applications, IUserContext currentUser) : ICommandHandler<Command, bool>
    {
        public async Task<Result<bool>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            JobApplication? application = await applications.GetByIdAsync(command.ApplicationId, currentUser.UserId, cancellationToken);

            if (application is null)
            {
                return Result<bool>.Failure(ApplicationsErrors.JobApplicationNotFound);
            }

            JobInterview? interview = application.Interviews.SingleOrDefault(item => item.Id == command.InterviewId);

            if (interview is null)
            {
                return Result<bool>.Failure(ApplicationsErrors.InterviewNotFound);
            }

            interview.Update(
                command.Type,
                command.ScheduledAt,
                command.DurationMinutes,
                command.Format,
                command.Location,
                command.PreparationNotes,
                command.Participants.Select(participant => (participant.Name, participant.RoleTitle)));

            application.AddNote("Scheduled interview updated.");

            await applications.SaveAsync(application, cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
