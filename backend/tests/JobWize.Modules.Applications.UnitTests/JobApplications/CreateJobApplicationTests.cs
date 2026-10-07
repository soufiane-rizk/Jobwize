using FluentAssertions;
using FluentValidation.Results;
using JobWize.Modules.Applications.Application;
using JobWize.Modules.Applications.Application.JobApplications;
using JobWize.Modules.Applications.Contracts.Events.JobApplications;
using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using JobWize.Modules.Applications.Domain;
using JobWize.Modules.Applications.Persistence;
using JobWize.Modules.Companies.Contracts.Public.Companies;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;
using Microsoft.EntityFrameworkCore;
using CreateJobApplicationContract = JobWize.Modules.Applications.Contracts.Public.JobApplications.CreateJobApplication;
using CreateJobApplicationFeature = JobWize.Modules.Applications.Application.JobApplications.CreateJobApplication;

namespace JobWize.Modules.Applications.UnitTests.JobApplications;

public sealed class CreateJobApplicationTests
{
    [Fact]
    public void Validator_Should_Reject_Applied_Application_Without_AppliedOn()
    {
        var validator = new CreateJobApplicationFeature.Validator();

        ValidationResult validationResult = validator.Validate(
            new CreateJobApplicationFeature.Command(
                Guid.NewGuid(),
                null,
                "Backend developer",
                ApplicationKind.JobPosting,
                ApplicationStatus.Applied,
                null,
                null,
                null));

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(error => error.PropertyName == "AppliedOn");
    }

    [Fact]
    public void Create_Should_Reject_Applied_Application_Without_AppliedOn()
    {
        Action act = () => JobApplication.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "Backend developer",
            ApplicationKind.JobPosting,
            ApplicationStatus.Applied,
            null,
            null,
            null);

        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName("appliedOn");
    }

    [Fact]
    public async Task HandleAsync_Should_Save_Application_And_Publish_Event()
    {
        var candidateId = Guid.NewGuid();
        var repository = new FakeJobApplicationRepository();
        var dispatcher = new FakeDispatcher();

        var options = new DbContextOptionsBuilder<ApplicationsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new ApplicationsDbContext(options);

        var companyId = Guid.NewGuid();

        dbContext.CompanyProjections.Add(CompanyProjection.CreateOrUpdate(
            companyId,
            "Acme",
            CompanyVisibility.Shared,
            null,
            true));

        await dbContext.SaveChangesAsync();

        var handler = new CreateJobApplicationFeature.Handler(
            repository,
            dbContext,
            new FakeUserContext(candidateId),
            dispatcher);

        Result<CreateJobApplicationContract.Response> result = await handler.HandleAsync(
            new CreateJobApplicationFeature.Command(
                companyId,
                null,
                "Backend developer",
                ApplicationKind.JobPosting,
                ApplicationStatus.Applied,
                new DateOnly(2026, 8, 27),
                "https://example.com/jobs/backend-developer",
                "Applied with the backend CV."),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repository.SavedApplication.Should().NotBeNull();
        repository.SavedApplication!.CandidateId.Should().Be(candidateId);
        repository.SavedApplication.CompanyId.Should().Be(companyId);
        repository.SavedApplication.LegacyCompanyName.Should().BeNull();

        dispatcher.PublishedNotification.Should().BeOfType<JobApplicationCreated>()
            .Which.JobApplicationId.Should().Be(result.Value.Id);
    }

    [Fact]
    public async Task HandleAsync_Should_Reject_A_Company_Outside_The_Local_Projection()
    {
        var candidateId = Guid.NewGuid();
        var repository = new FakeJobApplicationRepository();
        var dispatcher = new FakeDispatcher();

        var options = new DbContextOptionsBuilder<ApplicationsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new ApplicationsDbContext(options);

        var handler = new CreateJobApplicationFeature.Handler(
            repository,
            dbContext,
            new FakeUserContext(candidateId),
            dispatcher);

        Result<CreateJobApplicationContract.Response> result = await handler.HandleAsync(
            new CreateJobApplicationFeature.Command(
                Guid.NewGuid(),
                null,
                "Backend developer",
                ApplicationKind.JobPosting,
                ApplicationStatus.Planned,
                null,
                null,
                null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ApplicationsErrors.CompanyNotAvailable);
        repository.SavedApplication.Should().BeNull();
        dispatcher.PublishedNotification.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_Should_Reject_A_Location_That_Does_Not_Belong_To_The_Company()
    {
        var candidateId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var otherCompanyId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var repository = new FakeJobApplicationRepository();
        var dispatcher = new FakeDispatcher();

        var options = new DbContextOptionsBuilder<ApplicationsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new ApplicationsDbContext(options);

        dbContext.CompanyProjections.AddRange(
            CompanyProjection.CreateOrUpdate(
                companyId,
                "Acme",
                CompanyVisibility.Shared,
                null,
                true),
            CompanyProjection.CreateOrUpdate(
                otherCompanyId,
                "Other",
                CompanyVisibility.Shared,
                null,
                true));

        dbContext.CompanyLocationProjections.Add(CompanyLocationProjection.Create(
            locationId,
            otherCompanyId,
            "Other HQ",
            CompanyLocationVisibility.Shared,
            null,
            true));

        await dbContext.SaveChangesAsync();

        var handler = new CreateJobApplicationFeature.Handler(
            repository,
            dbContext,
            new FakeUserContext(candidateId),
            dispatcher);

        Result<CreateJobApplicationContract.Response> result = await handler.HandleAsync(
            new CreateJobApplicationFeature.Command(
                companyId,
                locationId,
                "Backend developer",
                ApplicationKind.JobPosting,
                ApplicationStatus.Planned,
                null,
                null,
                null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ApplicationsErrors.CompanyLocationNotAvailable);
        repository.SavedApplication.Should().BeNull();
        dispatcher.PublishedNotification.Should().BeNull();
    }

    private sealed class FakeJobApplicationRepository : IJobApplicationRepository
    {
        public JobApplication? SavedApplication { get; private set; }

        public Task<JobApplication?> GetByIdAsync(
            Guid applicationId,
            Guid candidateId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<JobApplication?>(null);
        }

        public Task SaveAsync(
            JobApplication application,
            CancellationToken cancellationToken = default)
        {
            SavedApplication = application;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserContext(Guid userId) : IUserContext
    {
        public Guid UserId { get; } = userId;
    }

    private sealed class FakeDispatcher : IDispatcher
    {
        public INotification? PublishedNotification { get; private set; }

        public Task<TResponse> SendAsync<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<TResponse> SendModuleQueryAsync<TResponse>(
            IModuleQuery<TResponse> query,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task PublishAsync(
            INotification notification,
            CancellationToken cancellationToken = default)
        {
            PublishedNotification = notification;
            return Task.CompletedTask;
        }
    }
}
