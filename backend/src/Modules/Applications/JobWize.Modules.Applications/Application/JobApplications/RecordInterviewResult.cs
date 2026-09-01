using FluentValidation;
using JobWize.Modules.Applications.Contracts.Events.JobApplications;
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
using RecordInterviewResultContract = JobWize.Modules.Applications.Contracts.Public.Interviews.RecordInterviewResult;

namespace JobWize.Modules.Applications.Application.JobApplications;

public static class RecordInterviewResult
{
    internal sealed record Command(
        Guid ApplicationId,
        Guid InterviewId,
        InterviewState State,
        DateTime? RescheduledAt,
        string? Note) : ICommand<bool>;

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.State)
                .Must(state => state is not InterviewState.Scheduled)
                .WithMessage("Select completed, cancelled, or postponed.");

            RuleFor(command => command.Note)
                .MaximumLength(4000);

            RuleFor(command => command.RescheduledAt)
                .NotNull()
                .When(command => command.State == InterviewState.Postponed);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    RecordInterviewResultContract.Route,
                    async (
                        Guid applicationId,
                        Guid interviewId,
                        RecordInterviewResultContract.Request request,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<bool> result = await dispatcher.SendAsync(
                            new Command(applicationId, interviewId, request.State, request.RescheduledAt, request.Note),
                            cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("RecordInterviewResult")
                .WithTags("Interviews");
        }
    }

    internal sealed class Handler(
        IJobApplicationRepository applications,
        IUserContext currentUser,
        IDispatcher dispatcher)
        : ICommandHandler<Command, bool>
    {
        public async Task<Result<bool>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            JobApplication? application = await applications.GetByIdAsync(
                command.ApplicationId,
                currentUser.UserId,
                cancellationToken);

            if (application is null)
            {
                return Result<bool>.Failure(ApplicationsErrors.JobApplicationNotFound);
            }

            application.RecordInterviewResult(
                command.InterviewId,
                command.State,
                command.RescheduledAt,
                command.Note);

            await applications.SaveAsync(application, cancellationToken);

            await dispatcher.PublishAsync(
                new JobInterviewResultRecorded(
                    command.InterviewId,
                    application.Id,
                    currentUser.UserId,
                    command.State),
                cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
