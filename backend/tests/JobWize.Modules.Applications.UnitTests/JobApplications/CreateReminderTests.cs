using FluentAssertions;
using JobWize.Modules.Applications.Application;
using JobWize.Modules.Applications.Contracts.Events.JobApplications;
using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using JobWize.Modules.Applications.Contracts.Public.Reminders;
using JobWize.Modules.Applications.Domain;
using JobWize.Modules.Applications.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Security;
using JobWize.Shared.Errors;
using CreateReminderFeature = JobWize.Modules.Applications.Application.JobApplications.CreateReminder;
using UpdateReminderStateFeature = JobWize.Modules.Applications.Application.JobApplications.UpdateReminderState;

namespace JobWize.Modules.Applications.UnitTests.JobApplications;

public sealed class CreateReminderTests
{
    [Fact]
    public async Task HandleAsync_Should_Save_A_Valid_Custom_Reminder()
    {
        Guid candidateId = Guid.NewGuid();
        JobApplication application = CreateApplication(candidateId);
        var repository = new FakeJobApplicationRepository(application);
        var dispatcher = new FakeDispatcher();
        var handler = new CreateReminderFeature.Handler(
            repository,
            new FakeUserContext(candidateId),
            dispatcher);

        var result = await handler.HandleAsync(
            new CreateReminderFeature.Command(
                application.Id,
                ReminderKind.Custom,
                null,
                null,
                "Follow up",
                DateTime.UtcNow.AddDays(1),
                null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repository.WasSaved.Should().BeTrue();
        application.Reminders.Should().ContainSingle();
        dispatcher.PublishedNotification.Should().BeOfType<JobApplicationReminderCreated>();
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Invalid_Relation_For_Another_Submission()
    {
        Guid candidateId = Guid.NewGuid();
        JobApplication application = CreateApplication(candidateId);
        var repository = new FakeJobApplicationRepository(application);
        var dispatcher = new FakeDispatcher();
        var handler = new CreateReminderFeature.Handler(
            repository,
            new FakeUserContext(candidateId),
            dispatcher);

        Func<Task> action = () => handler.HandleAsync(
            new CreateReminderFeature.Command(
                application.Id,
                ReminderKind.CvSubmission,
                Guid.NewGuid(),
                null,
                "Resend CV",
                DateTime.UtcNow.AddDays(3),
                null),
            CancellationToken.None);

        (await action.Should().ThrowAsync<BusinessRuleException>())
            .Which.Error.Should().Be(DomainErrors.CvSubmissionNotInApplication);
        repository.WasSaved.Should().BeFalse();
        dispatcher.PublishedNotification.Should().BeNull();
    }

    [Fact]
    public async Task UpdateState_Should_Complete_An_Open_Reminder()
    {
        Guid candidateId = Guid.NewGuid();
        JobApplication application = CreateApplication(candidateId);
        JobApplicationReminder reminder = application.CreateReminder(
            ReminderKind.Custom,
            null,
            null,
            "Follow up",
            DateTime.UtcNow.AddDays(1),
            null);
        var repository = new FakeJobApplicationRepository(application);
        var dispatcher = new FakeDispatcher();
        var handler = new UpdateReminderStateFeature.Handler(
            repository,
            new FakeUserContext(candidateId),
            dispatcher);

        var result = await handler.HandleAsync(
            new UpdateReminderStateFeature.Command(
                application.Id,
                reminder.Id,
                ReminderState.Completed),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        reminder.State.Should().Be(ReminderState.Completed);
        repository.WasSaved.Should().BeTrue();
        dispatcher.PublishedNotification.Should().BeOfType<JobApplicationReminderStateChanged>();
    }

    [Fact]
    public async Task UpdateState_Should_Return_Reminder_Not_Found_For_Another_Reminder()
    {
        Guid candidateId = Guid.NewGuid();
        JobApplication application = CreateApplication(candidateId);
        var repository = new FakeJobApplicationRepository(application);
        var dispatcher = new FakeDispatcher();
        var handler = new UpdateReminderStateFeature.Handler(
            repository,
            new FakeUserContext(candidateId),
            dispatcher);

        var result = await handler.HandleAsync(
            new UpdateReminderStateFeature.Command(
                application.Id,
                Guid.NewGuid(),
                ReminderState.Dismissed),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationsErrors.ReminderNotFound);
        repository.WasSaved.Should().BeFalse();
        dispatcher.PublishedNotification.Should().BeNull();
    }

    [Fact]
    public async Task UpdateState_Should_Return_Failure_When_Reminder_Is_Already_Closed()
    {
        Guid candidateId = Guid.NewGuid();
        JobApplication application = CreateApplication(candidateId);
        JobApplicationReminder reminder = application.CreateReminder(
            ReminderKind.Custom,
            null,
            null,
            "Follow up",
            DateTime.UtcNow.AddDays(1),
            null);
        application.ChangeReminderState(reminder.Id, ReminderState.Completed);
        var repository = new FakeJobApplicationRepository(application);
        var dispatcher = new FakeDispatcher();
        var handler = new UpdateReminderStateFeature.Handler(
            repository,
            new FakeUserContext(candidateId),
            dispatcher);

        Func<Task> action = () => handler.HandleAsync(
            new UpdateReminderStateFeature.Command(
                application.Id,
                reminder.Id,
                ReminderState.Dismissed),
            CancellationToken.None);

        (await action.Should().ThrowAsync<BusinessRuleException>())
            .Which.Error.Should().Be(DomainErrors.ReminderCannotChangeState);
        reminder.State.Should().Be(ReminderState.Completed);
        repository.WasSaved.Should().BeFalse();
        dispatcher.PublishedNotification.Should().BeNull();
    }

    private static JobApplication CreateApplication(Guid candidateId)
    {
        return JobApplication.Create(
            candidateId,
            Guid.NewGuid(),
            null,
            "Backend developer",
            ApplicationKind.JobPosting,
            ApplicationStatus.InProcess,
            new DateOnly(2026, 9, 1),
            null,
            null);
    }

    private sealed class FakeJobApplicationRepository(JobApplication application)
        : IJobApplicationRepository
    {
        public bool WasSaved { get; private set; }

        public Task<JobApplication?> GetByIdAsync(
            Guid applicationId,
            Guid candidateId,
            CancellationToken cancellationToken = default)
        {
            JobApplication? result = application.Id == applicationId &&
                                     application.CandidateId == candidateId
                ? application
                : null;

            return Task.FromResult(result);
        }

        public Task SaveAsync(
            JobApplication item,
            CancellationToken cancellationToken = default)
        {
            WasSaved = true;
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
