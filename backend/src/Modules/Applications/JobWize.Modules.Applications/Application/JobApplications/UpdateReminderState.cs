using FluentValidation;
using JobWize.Modules.Applications.Contracts.Events.JobApplications;
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

namespace JobWize.Modules.Applications.Application.JobApplications;

public static class UpdateReminderState
{
    internal sealed record Command(Guid ApplicationId, Guid ReminderId, ReminderState State)
        : ICommand<bool>;

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.State)
                .Must(state => state is ReminderState.Completed or ReminderState.Dismissed)
                .WithMessage("Select completed or dismissed as the reminder state.");
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch(
                    Contracts.Public.Reminders.UpdateReminderState.Route,
                    async (
                        Guid applicationId,
                        Guid reminderId,
                        Contracts.Public.Reminders.UpdateReminderState.Request request,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<bool> result = await dispatcher.SendAsync(
                            new Command(applicationId, reminderId, request.State),
                            cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("UpdateReminderState")
                .WithTags("Reminders");
        }
    }

    internal sealed class Handler(
        IJobApplicationRepository applications,
        IUserContext user,
        IDispatcher dispatcher)
        : ICommandHandler<Command, bool>
    {
        public async Task<Result<bool>> HandleAsync(
            Command command,
            CancellationToken cancellationToken)
        {
            Domain.JobApplication? application = await applications.GetByIdAsync(
                command.ApplicationId,
                user.UserId,
                cancellationToken);

            if (application is null)
            {
                return Result<bool>.Failure(ApplicationsErrors.JobApplicationNotFound);
            }

            Domain.JobApplicationReminder? reminder = application.Reminders
                .SingleOrDefault(item => item.Id == command.ReminderId);

            if (reminder is null)
            {
                return Result<bool>.Failure(ApplicationsErrors.ReminderNotFound);
            }

            if (reminder.State != ReminderState.Open ||
                command.State is not (ReminderState.Completed or ReminderState.Dismissed))
            {
                return Result<bool>.Failure(ApplicationsErrors.ReminderCannotChangeState);
            }

            application.ChangeReminderState(command.ReminderId, command.State);

            await applications.SaveAsync(application, cancellationToken);

            await dispatcher.PublishAsync(
                new JobApplicationReminderStateChanged(
                    reminder.Id,
                    application.Id,
                    user.UserId,
                    reminder.State),
                cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
