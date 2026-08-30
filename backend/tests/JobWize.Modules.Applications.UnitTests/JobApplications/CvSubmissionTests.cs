using FluentAssertions;
using JobWize.Modules.Applications.Application;
using JobWize.Modules.Applications.Contracts.Events.JobApplications;
using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using JobWize.Modules.Applications.Domain;
using JobWize.Modules.Applications.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;
using Microsoft.EntityFrameworkCore;
using RecordCvSubmissionFeature = JobWize.Modules.Applications.Application.JobApplications.RecordCvSubmission;

namespace JobWize.Modules.Applications.UnitTests.JobApplications;

public sealed class CvSubmissionTests
{
    [Fact]
    public void RecordCvSubmission_Should_Create_Snapshots_And_Mark_Planned_Application_Applied()
    {
        JobApplication application = CreateApplication(ApplicationStatus.Planned);
        DateTime sentAt = new(2024, 1, 15, 9, 30, 0, DateTimeKind.Utc);
        Guid fileId = Guid.NewGuid();
        Guid contactId = Guid.NewGuid();
        DateTime recordedAfter = DateTime.UtcNow;

        JobApplicationCvSubmission submission = application.RecordCvSubmission(
            sentAt,
            CvSubmissionMethod.Email,
            "Sent after speaking with the recruiter.",
            (contactId, null, "Jane Recruiter", "Recruiter", "jane@example.com", null),
            [(fileId, "backend-cv.pdf", "application/pdf", 1024)]);

        application.Status.Should().Be(ApplicationStatus.Applied);
        application.AppliedOn.Should().Be(new DateOnly(2024, 1, 15));
        application.CvSubmissions.Should().ContainSingle().Which.Should().BeSameAs(submission);
        submission.SentAt.Should().Be(sentAt);
        submission.ContactName.Should().Be("Jane Recruiter");
        submission.Documents.Should().ContainSingle().Which.FileId.Should().Be(fileId);
        application.Activities.Should().Contain(activity =>
            activity.Type == ApplicationActivityType.CvSubmitted &&
            activity.OccurredAt >= recordedAfter &&
            activity.OccurredAt != sentAt);
        application.Activities.Should().Contain(activity =>
            activity.Type == ApplicationActivityType.StatusChanged &&
            activity.Status == ApplicationStatus.Applied &&
            activity.OccurredAt >= recordedAfter);
    }

    [Fact]
    public void RecordCvSubmission_Should_Reject_Duplicate_Documents()
    {
        JobApplication application = CreateApplication(ApplicationStatus.Applied);
        Guid fileId = Guid.NewGuid();

        Action act = () => application.RecordCvSubmission(
            DateTime.UtcNow,
            CvSubmissionMethod.JobPortal,
            null,
            (null, null, null, null, null, null),
            [
                (fileId, "cv.pdf", "application/pdf", 100),
                (fileId, "cv.pdf", "application/pdf", 100)
            ]);

        act.Should().Throw<ArgumentException>().WithParameterName("documents");
        application.CvSubmissions.Should().BeEmpty();
    }

    [Fact]
    public async Task RecordCvSubmission_Should_Track_New_Snapshot_Entities_As_Added()
    {
        await using ApplicationsDbContext dbContext = CreateDbContext();
        JobApplication application = CreateApplication(ApplicationStatus.Applied);
        dbContext.JobApplications.Add(application);
        await dbContext.SaveChangesAsync();

        JobApplicationCvSubmission submission = application.RecordCvSubmission(
            DateTime.UtcNow,
            CvSubmissionMethod.Email,
            null,
            (null, null, null, null, null, null),
            [(Guid.NewGuid(), "cv.pdf", "application/pdf", 100)]);

        dbContext.ChangeTracker.DetectChanges();

        dbContext.Entry(submission).State.Should().Be(EntityState.Added);
        dbContext.Entry(submission.Documents.Single()).State.Should().Be(EntityState.Added);
    }

    [Fact]
    public async Task HandleAsync_Should_Validate_Files_Save_Snapshots_And_Publish_Events()
    {
        Guid candidateId = Guid.NewGuid();
        Guid fileId = Guid.NewGuid();
        JobApplication application = CreateApplication(ApplicationStatus.Planned, candidateId);
        var repository = new FakeJobApplicationRepository(application);
        var dispatcher = new FakeDispatcher([
            new(fileId, "cv.pdf", "application/pdf", 2048)
        ]);

        await using ApplicationsDbContext dbContext = CreateDbContext();
        var handler = new RecordCvSubmissionFeature.Handler(
            repository,
            dbContext,
            new FakeUserContext(candidateId),
            dispatcher);

        Result<JobWize.Modules.Applications.Contracts.Public.JobApplications.RecordCvSubmission.Response> result =
            await handler.HandleAsync(
                new RecordCvSubmissionFeature.Command(
                    application.Id,
                    new DateTime(2026, 8, 30, 9, 30, 0, DateTimeKind.Utc),
                    CvSubmissionMethod.JobPortal,
                    [fileId],
                    null,
                    null),
                CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repository.Saved.Should().BeTrue();
        application.CvSubmissions.Should().ContainSingle();
        dispatcher.Published.Should().ContainSingle(item => item is JobApplicationCvSubmitted);
        dispatcher.Published.Should().ContainSingle(item => item is JobApplicationStatusChanged);
    }

    [Fact]
    public async Task HandleAsync_Should_Reject_A_Contact_From_Another_Company()
    {
        Guid candidateId = Guid.NewGuid();
        Guid fileId = Guid.NewGuid();
        Guid contactId = Guid.NewGuid();
        JobApplication application = CreateApplication(ApplicationStatus.Applied, candidateId);
        var repository = new FakeJobApplicationRepository(application);
        var dispatcher = new FakeDispatcher([
            new(fileId, "cv.pdf", "application/pdf", 2048)
        ]);

        await using ApplicationsDbContext dbContext = CreateDbContext();
        dbContext.CompanyContactProjections.Add(CompanyContactProjection.Create(
            contactId,
            Guid.NewGuid(),
            null,
            "Other contact",
            null,
            null,
            null,
            JobWize.Modules.Companies.Contracts.Public.CompanyContacts.CompanyContactVisibility.Shared,
            null,
            true,
            false));
        await dbContext.SaveChangesAsync();

        var handler = new RecordCvSubmissionFeature.Handler(
            repository,
            dbContext,
            new FakeUserContext(candidateId),
            dispatcher);

        Result<JobWize.Modules.Applications.Contracts.Public.JobApplications.RecordCvSubmission.Response> result =
            await handler.HandleAsync(
                new RecordCvSubmissionFeature.Command(
                    application.Id,
                    DateTime.UtcNow,
                    CvSubmissionMethod.Email,
                    [fileId],
                    contactId,
                    null),
                CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationsErrors.CompanyContactNotAvailable);
        repository.Saved.Should().BeFalse();
    }

    private static JobApplication CreateApplication(
        ApplicationStatus status,
        Guid? candidateId = null)
    {
        return JobApplication.Create(
            candidateId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "Backend developer",
            ApplicationKind.JobPosting,
            status,
            status == ApplicationStatus.Planned ? null : new DateOnly(2026, 8, 30),
            null,
            null);
    }

    private static ApplicationsDbContext CreateDbContext()
    {
        DbContextOptions<ApplicationsDbContext> options =
            new DbContextOptionsBuilder<ApplicationsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new ApplicationsDbContext(options);
    }

    private sealed class FakeJobApplicationRepository(JobApplication application)
        : IJobApplicationRepository
    {
        public bool Saved { get; private set; }

        public Task<JobApplication?> GetByIdAsync(
            Guid applicationId,
            Guid candidateId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<JobApplication?>(
                application.Id == applicationId && application.CandidateId == candidateId
                    ? application
                    : null);
        }

        public Task SaveAsync(
            JobApplication jobApplication,
            CancellationToken cancellationToken = default)
        {
            Saved = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserContext(Guid userId) : IUserContext
    {
        public Guid UserId { get; } = userId;
    }

    private sealed class FakeDispatcher(
        IReadOnlyList<JobWize.Modules.Files.Contracts.Internal.FileAssets.GetCandidateDocumentsForSubmission.Item> files)
        : IDispatcher
    {
        public List<INotification> Published { get; } = [];

        public Task<TResponse> SendAsync<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<TResponse> SendModuleQueryAsync<TResponse>(
            IModuleQuery<TResponse> query,
            CancellationToken cancellationToken = default)
        {
            object response = new JobWize.Modules.Files.Contracts.Internal.FileAssets.GetCandidateDocumentsForSubmission.Response(files);
            return Task.FromResult((TResponse)response);
        }

        public Task PublishAsync(
            INotification notification,
            CancellationToken cancellationToken = default)
        {
            Published.Add(notification);
            return Task.CompletedTask;
        }
    }
}
