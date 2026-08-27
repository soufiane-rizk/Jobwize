using FluentAssertions;
using FluentValidation.Results;
using JobWize.Modules.Applications.Application.JobApplications;
using JobWize.Modules.Applications.Contracts.Events.JobApplications;
using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using JobWize.Modules.Applications.Domain;
using JobWize.Modules.Applications.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;
using CreateJobApplicationContract = JobWize.Modules.Applications.Contracts.Public.JobApplications.CreateJobApplication;
using CreateJobApplicationFeature = JobWize.Modules.Applications.Application.JobApplications.CreateJobApplication;

namespace JobWize.Applications.UnitTests.JobApplications;

public sealed class CreateJobApplicationTests
{
    [Fact]
    public void Validator_Should_Reject_Applied_Application_Without_AppliedOn()
    {
        var validator = new CreateJobApplicationFeature.Validator();

        ValidationResult validationResult = validator.Validate(
            new CreateJobApplicationFeature.Command(
                "Acme",
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
            "Acme",
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
        var handler = new CreateJobApplicationFeature.Handler(
            repository,
            new FakeUserContext(candidateId),
            dispatcher);

        Result<CreateJobApplicationContract.Response> result = await handler.HandleAsync(
            new CreateJobApplicationFeature.Command(
                "Acme",
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
        repository.SavedApplication.CompanyName.Should().Be("Acme");

        dispatcher.PublishedNotification.Should().BeOfType<JobApplicationCreated>()
            .Which.JobApplicationId.Should().Be(result.Value.Id);
    }

    private sealed class FakeJobApplicationRepository : IJobApplicationRepository
    {
        public JobApplication? SavedApplication { get; private set; }

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
