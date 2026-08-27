using FluentValidation;
using JobWize.Modules.Applications.Contracts.Events.JobApplications;
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

namespace JobWize.Modules.Applications.Application.JobApplications;

public static class ChangeJobApplicationStatus
{
    internal sealed record Command(
        Guid Id,
        ApplicationStatus Status,
        DateOnly? AppliedOn,
        string? Note) : ICommand<bool>;

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Note)
                .MaximumLength(4000);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch(
                    Contracts.Public.JobApplications.ChangeJobApplicationStatus.Route,
                    async (
                        Guid id,
                        Contracts.Public.JobApplications.ChangeJobApplicationStatus.Request request,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<bool> result = await dispatcher.SendAsync(
                            new Command(id, request.Status, request.AppliedOn, request.Note),
                            cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("ChangeJobApplicationStatus")
                .WithTags("Job applications");
        }
    }

    internal sealed class Handler(
        IJobApplicationRepository jobApplications,
        IUserContext userContext,
        IDispatcher dispatcher) : ICommandHandler<Command, bool>
    {
        public async Task<Result<bool>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            Domain.JobApplication? application = await jobApplications.GetByIdAsync(
                command.Id,
                userContext.UserId,
                cancellationToken);

            if (application is null)
            {
                return Result<bool>.Failure(ApplicationsErrors.JobApplicationNotFound);
            }

            application.ChangeStatus(command.Status, command.AppliedOn, command.Note);

            await jobApplications.SaveAsync(application, cancellationToken);

            await dispatcher.PublishAsync(
                new JobApplicationStatusChanged(application.Id, userContext.UserId, application.Status),
                cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
