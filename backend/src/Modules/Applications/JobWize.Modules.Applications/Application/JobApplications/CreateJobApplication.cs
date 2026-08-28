using FluentValidation;
using JobWize.Modules.Applications.Contracts.Events.JobApplications;
using JobWize.Modules.Applications.Contracts.Public.JobApplications;
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
public static class CreateJobApplication
{
    internal sealed record Command(
        Guid CompanyId,
        Guid? CompanyLocationId,
        string? RoleTitle,
        ApplicationKind Kind,
        ApplicationStatus Status,
        DateOnly? AppliedOn,
        string? SourceUrl,
        string? Notes) : ICommand<Contracts.Public.JobApplications.CreateJobApplication.Response>;

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.RoleTitle)
                .MaximumLength(200);

            RuleFor(x => x.AppliedOn)
                .NotNull()
                .When(x => x.Status == ApplicationStatus.Applied)
                .WithMessage("Applied on is required when the application has been sent.");

            RuleFor(x => x.SourceUrl)
                .MaximumLength(2048)
                .Must(url => string.IsNullOrWhiteSpace(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Source URL must be an absolute URL.");

            RuleFor(x => x.Notes)
                .MaximumLength(8000);
        }
    }
    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    Contracts.Public.JobApplications.CreateJobApplication.Route,
                    async (
                        Contracts.Public.JobApplications.CreateJobApplication.Request request,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        var command = new Command(
                            request.CompanyId,
                            request.CompanyLocationId,
                            request.RoleTitle,
                            request.Kind,
                            request.Status,
                            request.AppliedOn,
                            request.SourceUrl,
                            request.Notes);

                        Result<Contracts.Public.JobApplications.CreateJobApplication.Response> result =
                            await dispatcher.SendAsync(command, cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("CreateJobApplication")
                .WithTags("Job applications");
        }
    }

    internal sealed class Handler(
        IJobApplicationRepository jobApplications,
        ApplicationsDbContext dbContext,
        IUserContext userContext,
        IDispatcher dispatcher) : ICommandHandler<Command, Contracts.Public.JobApplications.CreateJobApplication.Response>
    {
        public async Task<Result<Contracts.Public.JobApplications.CreateJobApplication.Response>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            Domain.CompanyProjection? company = await dbContext.CompanyProjections
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.Id == command.CompanyId, cancellationToken);

            if (company is null ||
                !company.IsActive ||
                (company.Visibility != JobWize.Modules.Companies.Contracts.Public.Companies.CompanyVisibility.Shared &&
                 company.CreatedByCandidateId != userContext.UserId))
            {
                return Result<Contracts.Public.JobApplications.CreateJobApplication.Response>.Failure(
                    ApplicationsErrors.CompanyNotAvailable);
            }

            if (command.CompanyLocationId is Guid locationId)
            {
                bool locationIsAvailable = await dbContext.CompanyLocationProjections
                    .AsNoTracking()
                    .AnyAsync(
                        item => item.Id == locationId &&
                                item.CompanyId == command.CompanyId &&
                                item.IsActive,
                        cancellationToken);

                if (!locationIsAvailable)
                {
                    return Result<Contracts.Public.JobApplications.CreateJobApplication.Response>.Failure(
                        ApplicationsErrors.CompanyLocationNotAvailable);
                }
            }

            JobApplication application = JobApplication.Create(
                userContext.UserId,
                command.CompanyId,
                command.CompanyLocationId,
                command.RoleTitle,
                command.Kind,
                command.Status,
                command.AppliedOn,
                command.SourceUrl,
                command.Notes);

            await jobApplications.SaveAsync(application, cancellationToken);

            await dispatcher.PublishAsync(
                new JobApplicationCreated(application.Id, userContext.UserId),
                cancellationToken);

            return Result<Contracts.Public.JobApplications.CreateJobApplication.Response>.Success(new(application.Id));
        }
    }
}
