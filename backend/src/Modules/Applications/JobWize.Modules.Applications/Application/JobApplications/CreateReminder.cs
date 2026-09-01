using FluentValidation;
using JobWize.Modules.Applications.Contracts.Events.JobApplications;
using JobWize.Modules.Applications.Contracts.Public.Reminders;
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

namespace JobWize.Modules.Applications.Application.JobApplications;

public static class CreateReminder
{
    internal sealed record Command(
        Guid ApplicationId,
        ReminderKind Kind,
        Guid? CvSubmissionId,
        Guid? InterviewId,
        string Title,
        DateTime DueAt,
        string? Note)
        : ICommand<Contracts.Public.Reminders.CreateReminder.Response>;

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Kind).IsInEnum();
            RuleFor(command => command.Title).NotEmpty().MaximumLength(200);
            RuleFor(command => command.DueAt).NotEqual(default(DateTime));
            RuleFor(command => command.Note).MaximumLength(4000);
            RuleFor(command => command)
                .Must(HasValidRelation)
                .WithMessage("Select exactly the related CV submission or interview required by the reminder type.");
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    Contracts.Public.Reminders.CreateReminder.Route,
                    async (
                        Guid id,
                        Contracts.Public.Reminders.CreateReminder.Request request,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<Contracts.Public.Reminders.CreateReminder.Response> result =
                            await dispatcher.SendAsync(
                                new Command(
                                    id,
                                    request.Kind,
                                    request.CvSubmissionId,
                                    request.InterviewId,
                                    request.Title,
                                    request.DueAt,
                                    request.Note),
                                cancellationToken);
                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("CreateReminder")
                .WithTags("Reminders");
        }
    }

    internal sealed class Handler(
        IJobApplicationRepository applications,
        IUserContext user,
        IDispatcher dispatcher)
        : ICommandHandler<Command, Contracts.Public.Reminders.CreateReminder.Response>
    {
        public async Task<Result<Contracts.Public.Reminders.CreateReminder.Response>> HandleAsync(
            Command command,
            CancellationToken cancellationToken)
        {
            JobApplication? application = await applications.GetByIdAsync(
                command.ApplicationId,
                user.UserId,
                cancellationToken);

            if (application is null)
            {
                return Result<Contracts.Public.Reminders.CreateReminder.Response>.Failure(
                    ApplicationsErrors.JobApplicationNotFound);
            }

            if (!HasValidRelation(command) ||
                (command.CvSubmissionId is Guid cvSubmissionId &&
                 !application.CvSubmissions.Any(item => item.Id == cvSubmissionId)) ||
                (command.InterviewId is Guid interviewId &&
                 !application.Interviews.Any(item => item.Id == interviewId)))
            {
                return Result<Contracts.Public.Reminders.CreateReminder.Response>.Failure(
                    ApplicationsErrors.InvalidReminderRelation);
            }

            JobApplicationReminder reminder = application.CreateReminder(
                command.Kind,
                command.CvSubmissionId,
                command.InterviewId,
                command.Title,
                command.DueAt,
                command.Note);

            await applications.SaveAsync(application, cancellationToken);

            await dispatcher.PublishAsync(
                new JobApplicationReminderCreated(
                    reminder.Id,
                    application.Id,
                    user.UserId,
                    reminder.Kind,
                    reminder.DueAt),
                cancellationToken);

            return Result<Contracts.Public.Reminders.CreateReminder.Response>.Success(
                new(reminder.Id));
        }
    }

    private static bool HasValidRelation(Command command) =>
        command.Kind switch
        {
            ReminderKind.CvSubmission => command.CvSubmissionId is not null && command.InterviewId is null,
            ReminderKind.Interview => command.CvSubmissionId is null && command.InterviewId is not null,
            ReminderKind.Custom => command.CvSubmissionId is null && command.InterviewId is null,
            _ => false
        };
}
