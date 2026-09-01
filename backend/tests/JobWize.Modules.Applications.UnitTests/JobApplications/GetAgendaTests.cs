using FluentAssertions;
using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using JobWize.Modules.Applications.Contracts.Public.Interviews;
using JobWize.Modules.Applications.Contracts.Public.Reminders;
using JobWize.Modules.Applications.Domain;
using JobWize.Modules.Applications.Persistence;
using JobWize.Modules.Companies.Contracts.Public.Companies;
using JobWize.Shared.Application.Security;
using Microsoft.EntityFrameworkCore;
using GetAgendaFeature = JobWize.Modules.Applications.Application.JobApplications.GetAgenda;

namespace JobWize.Modules.Applications.UnitTests.JobApplications;

public sealed class GetAgendaTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Interviews_And_Open_Reminders_For_The_Candidate()
    {
        Guid candidateId = Guid.NewGuid();
        Guid companyId = Guid.NewGuid();
        DateTime interviewAt = new(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc);
        DateTime reminderAt = new(2026, 9, 9, 9, 0, 0, DateTimeKind.Utc);
        JobApplication application = JobApplication.Create(
            candidateId,
            companyId,
            null,
            "Backend developer",
            ApplicationKind.JobPosting,
            ApplicationStatus.InProcess,
            new DateOnly(2026, 9, 1),
            null,
            null);
        application.ScheduleInterview(
            InterviewType.Technical,
            interviewAt,
            60,
            InterviewFormat.Video,
            null,
            null,
            []);
        application.CreateReminder(
            ReminderKind.Custom,
            null,
            null,
            "Send thank-you email",
            reminderAt,
            null);
        JobApplicationReminder closedReminder = application.CreateReminder(
            ReminderKind.Custom,
            null,
            null,
            "Already completed",
            reminderAt,
            null);
        application.ChangeReminderState(closedReminder.Id, ReminderState.Completed);

        JobApplication anotherCandidateApplication = JobApplication.Create(
            Guid.NewGuid(),
            companyId,
            null,
            "Frontend developer",
            ApplicationKind.JobPosting,
            ApplicationStatus.InProcess,
            new DateOnly(2026, 9, 1),
            null,
            null);
        anotherCandidateApplication.CreateReminder(
            ReminderKind.Custom,
            null,
            null,
            "Another candidate's reminder",
            reminderAt,
            null);

        var options = new DbContextOptionsBuilder<ApplicationsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var dbContext = new ApplicationsDbContext(options);
        dbContext.CompanyProjections.Add(CompanyProjection.CreateOrUpdate(
            companyId,
            "Atlas Studio",
            CompanyVisibility.Shared,
            null,
            true));
        dbContext.JobApplications.Add(application);
        dbContext.JobApplications.Add(anotherCandidateApplication);
        await dbContext.SaveChangesAsync();

        var handler = new GetAgendaFeature.Handler(
            dbContext,
            new FakeUserContext(candidateId));

        var result = await handler.HandleAsync(
            new GetAgendaFeature.Query(
                new DateTime(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 9, 14, 0, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Should().OnlyContain(item => item.CompanyName == "Atlas Studio");
        result.Value.Items.Should().Contain(item => item.InterviewState == InterviewState.Scheduled);
        result.Value.Items.Should().Contain(item => item.Title == "Send thank-you email");
        result.Value.Items.Should().NotContain(item => item.Title == "Already completed");
        result.Value.Items.Should().NotContain(item => item.Title == "Another candidate's reminder");
    }

    private sealed class FakeUserContext(Guid userId) : IUserContext
    {
        public Guid UserId { get; } = userId;
    }
}
