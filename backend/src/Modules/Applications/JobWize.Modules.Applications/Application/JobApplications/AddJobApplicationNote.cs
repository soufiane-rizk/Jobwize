using FluentValidation;
using JobWize.Modules.Applications.Contracts.Events.JobApplications;
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

public static class AddJobApplicationNote
{
    internal sealed record Command(Guid Id, string Note) : ICommand<bool>;

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Note)
                .NotEmpty()
                .MaximumLength(4000);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    Contracts.Public.JobApplications.AddJobApplicationNote.Route,
                    async (
                        Guid id,
                        Contracts.Public.JobApplications.AddJobApplicationNote.Request request,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<bool> result = await dispatcher.SendAsync(
                            new Command(id, request.Note),
                            cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("AddJobApplicationNote")
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

            application.AddNote(command.Note);

            await jobApplications.SaveAsync(application, cancellationToken);

            await dispatcher.PublishAsync(
                new JobApplicationNoteAdded(application.Id, userContext.UserId),
                cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
