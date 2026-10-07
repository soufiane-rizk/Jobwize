using FluentAssertions;
using JobWize.Modules.Applications.Application.JobApplications;
using JobWize.Modules.Applications.Contracts.Events.JobApplications;
using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using JobWize.Modules.Applications.Contracts.Public.Interviews;
using JobWize.Modules.Applications.Domain;
using JobWize.Modules.Applications.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Security;
using ScheduleInterviewFeature = JobWize.Modules.Applications.Application.JobApplications.ScheduleInterview;

namespace JobWize.Applications.UnitTests.JobApplications;

public sealed class ScheduleInterviewTests
{
    [Fact]
    public async Task HandleAsync_Should_Start_Process_Save_Interview_And_Publish_Event()
    {
        var candidateId = Guid.NewGuid();
        JobApplication application = JobApplication.Create(
            candidateId,
            Guid.NewGuid(),
            null,
            "Backend developer",
            ApplicationKind.JobPosting,
            ApplicationStatus.Applied,
            new DateOnly(2026, 8, 27),
            null,
            null);
        var applications = new FakeJobApplicationRepository(application);
        var dispatcher = new FakeDispatcher();
        var handler = new ScheduleInterviewFeature.Handler(
            applications,
            new FakeUserContext(candidateId),
            dispatcher);

        await handler.HandleAsync(
            new ScheduleInterviewFeature.Command(
                application.Id,
                InterviewType.Technical,
                new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc),
                60,
                InterviewFormat.Video,
                "https://example.com/meeting",
                null,
                [new("Jane Doe", "Engineering manager")]),
            CancellationToken.None);

        application.Status.Should().Be(ApplicationStatus.InProcess);
        application.Interviews.Should().ContainSingle();
        application.Interviews.Single().Participants.Should().ContainSingle();
        dispatcher.PublishedNotification.Should().BeOfType<JobInterviewScheduled>()
            .Which.JobApplicationId.Should().Be(application.Id);
    }

    private sealed class FakeJobApplicationRepository(JobApplication application) : IJobApplicationRepository
    {
        public Task<JobApplication?> GetByIdAsync(
            Guid applicationId,
            Guid candidateId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                application.Id == applicationId && application.CandidateId == candidateId
                    ? application
                    : null);
        }

        public Task SaveAsync(JobApplication item, CancellationToken cancellationToken = default)
        {
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

        public Task PublishAsync(INotification notification, CancellationToken cancellationToken = default)
        {
            PublishedNotification = notification;
            return Task.CompletedTask;
        }
    }
}
