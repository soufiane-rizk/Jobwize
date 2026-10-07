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
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Applications.Application.JobApplications;

public static class RecordCvSubmission
{
    internal sealed record Command(
        Guid Id,
        DateTime SentAt,
        CvSubmissionMethod Method,
        IReadOnlyList<Guid> FileIds,
        Guid? CompanyContactId,
        string? Notes)
        : ICommand<Contracts.Public.JobApplications.RecordCvSubmission.Response>;

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.SentAt).NotEmpty();
            RuleFor(command => command.Method).IsInEnum();
            RuleFor(command => command.FileIds)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(ids => ids.Count <= 10)
                .WithMessage("A submission can contain at most 10 documents.")
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage("A document can only be selected once.");
            RuleFor(command => command.Notes).MaximumLength(4000);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    Contracts.Public.JobApplications.RecordCvSubmission.Route,
                    async (
                        Guid id,
                        Contracts.Public.JobApplications.RecordCvSubmission.Request request,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<Contracts.Public.JobApplications.RecordCvSubmission.Response> result =
                            await dispatcher.SendAsync(
                                new Command(
                                    id,
                                    request.SentAt,
                                    request.Method,
                                    request.FileIds,
                                    request.CompanyContactId,
                                    request.Notes),
                                cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("RecordCvSubmission")
                .WithTags("Job applications");
        }
    }

    internal sealed class Handler(
        IJobApplicationRepository jobApplications,
        ApplicationsDbContext dbContext,
        IUserContext userContext,
        IDispatcher dispatcher)
        : ICommandHandler<Command, Contracts.Public.JobApplications.RecordCvSubmission.Response>
    {
        public async Task<Result<Contracts.Public.JobApplications.RecordCvSubmission.Response>> HandleAsync(
            Command command,
            CancellationToken cancellationToken)
        {
            Domain.JobApplication? application = await jobApplications.GetByIdAsync(
                command.Id,
                userContext.UserId,
                cancellationToken);

            if (application is null)
            {
                return Result<Contracts.Public.JobApplications.RecordCvSubmission.Response>.Failure(
                    ApplicationsErrors.JobApplicationNotFound);
            }

            JobWize.Modules.Files.Contracts.Internal.FileAssets.GetCandidateDocumentsForSubmission.Response files =
                await dispatcher.SendModuleQueryAsync(
                    new JobWize.Modules.Files.Contracts.Internal.FileAssets.GetCandidateDocumentsForSubmission.Query(
                        userContext.UserId,
                        command.FileIds),
                    cancellationToken);

            if (files.Files.Count != command.FileIds.Count)
            {
                return Result<Contracts.Public.JobApplications.RecordCvSubmission.Response>.Failure(
                    ApplicationsErrors.CandidateDocumentNotAvailable);
            }

            Domain.CompanyContactProjection? contact = null;

            if (command.CompanyContactId is not null)
            {
                contact = await dbContext.CompanyContactProjections
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        item =>
                            item.Id == command.CompanyContactId &&
                            item.CompanyId == application.CompanyId &&
                            (application.CompanyLocationId == null ||
                             item.CompanyLocationId == null ||
                             item.CompanyLocationId == application.CompanyLocationId) &&
                            item.IsActive &&
                            !item.IsRejected &&
                            (item.Visibility == JobWize.Modules.Companies.Contracts.Public.CompanyContacts.CompanyContactVisibility.Shared ||
                             item.CreatedByCandidateId == userContext.UserId),
                        cancellationToken);

                if (contact is null)
                {
                    return Result<Contracts.Public.JobApplications.RecordCvSubmission.Response>.Failure(
                        ApplicationsErrors.CompanyContactNotAvailable);
                }
            }

            bool statusChanged = application.Status is ApplicationStatus.Draft or ApplicationStatus.Planned;

            Domain.JobApplicationCvSubmission submission = application.RecordCvSubmission(
                command.SentAt,
                command.Method,
                command.Notes,
                (
                    contact?.Id,
                    contact?.CompanyLocationId,
                    contact?.Name,
                    contact?.RoleTitle,
                    contact?.Email,
                    contact?.PhoneNumber),
                files.Files.Select(file => (file.FileId, file.FileName, file.ContentType, file.SizeBytes)));

            await jobApplications.SaveAsync(application, cancellationToken);

            if (statusChanged)
            {
                await dispatcher.PublishAsync(
                    new JobApplicationStatusChanged(
                        application.Id,
                        userContext.UserId,
                        application.Status),
                    cancellationToken);
            }

            await dispatcher.PublishAsync(
                new JobApplicationCvSubmitted(
                    submission.Id,
                    application.Id,
                    userContext.UserId,
                    files.Files.Select(file => file.FileId).ToList()),
                cancellationToken);

            return Result<Contracts.Public.JobApplications.RecordCvSubmission.Response>.Success(
                new(submission.Id));
        }
    }
}
