using FluentAssertions;
using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using JobWize.Modules.Applications.Contracts.Public.Interviews;
using JobWize.Modules.Applications.Contracts.Public.Reminders;
using JobWize.Modules.Applications.Domain;

namespace JobWize.Modules.Applications.UnitTests.JobApplications;

public sealed class JobApplicationReminderTests
{
    [Fact]
    public void CreateReminder_Should_Create_An_Open_Custom_Reminder()
    {
        JobApplication application = CreateApplication();
        DateTime dueAt = new(2026, 9, 4, 9, 0, 0, DateTimeKind.Utc);

        JobApplicationReminder reminder = application.CreateReminder(
            ReminderKind.Custom,
            null,
            null,
            "Contact the recruiter",
            dueAt,
            "Ask whether the role is still open.");

        reminder.State.Should().Be(ReminderState.Open);
        reminder.Kind.Should().Be(ReminderKind.Custom);
        reminder.DueAt.Should().Be(dueAt);
        reminder.Title.Should().Be("Contact the recruiter");
        application.Reminders.Should().ContainSingle().Which.Should().BeSameAs(reminder);
    }

    [Fact]
    public void CreateReminder_Should_Require_The_Selected_Submission_To_Belong_To_The_Application()
    {
        JobApplication application = CreateApplication();

        Action action = () => application.CreateReminder(
            ReminderKind.CvSubmission,
            Guid.NewGuid(),
            null,
            "Resend CV",
            DateTime.UtcNow.AddDays(3),
            null);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateReminder_Should_Accept_A_Submission_From_The_Same_Application()
    {
        JobApplication application = CreateApplication();
        JobApplicationCvSubmission submission = application.RecordCvSubmission(
            new DateTime(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc),
            CvSubmissionMethod.Email,
            null,
            (null, null, null, null, null, null),
            [(Guid.NewGuid(), "Ahmed-CV.pdf", "application/pdf", 1024)]);

        JobApplicationReminder reminder = application.CreateReminder(
            ReminderKind.CvSubmission,
            submission.Id,
            null,
            "Resend CV",
            new DateTime(2026, 9, 8, 9, 0, 0, DateTimeKind.Utc),
            null);

        reminder.CvSubmissionId.Should().Be(submission.Id);
        reminder.InterviewId.Should().BeNull();
    }

    [Fact]
    public void CreateReminder_Should_Accept_An_Interview_From_The_Same_Application()
    {
        JobApplication application = CreateApplication();
        JobInterview interview = application.ScheduleInterview(
            InterviewType.Technical,
            new DateTime(2026, 9, 5, 10, 0, 0, DateTimeKind.Utc),
            60,
            InterviewFormat.Video,
            null,
            null,
            []);

        JobApplicationReminder reminder = application.CreateReminder(
            ReminderKind.Interview,
            null,
            interview.Id,
            "Prepare for interview",
            new DateTime(2026, 9, 3, 10, 0, 0, DateTimeKind.Utc),
            null);

        reminder.InterviewId.Should().Be(interview.Id);
        reminder.CvSubmissionId.Should().BeNull();
    }

    [Fact]
    public void ChangeReminderState_Should_Not_Allow_A_Closed_Reminder_To_Change_Again()
    {
        JobApplication application = CreateApplication();
        JobApplicationReminder reminder = application.CreateReminder(
            ReminderKind.Custom,
            null,
            null,
            "Follow up",
            DateTime.UtcNow.AddDays(1),
            null);

        application.ChangeReminderState(reminder.Id, ReminderState.Completed);
        Action action = () => application.ChangeReminderState(reminder.Id, ReminderState.Dismissed);

        reminder.State.Should().Be(ReminderState.Completed);
        action.Should().Throw<InvalidOperationException>();
    }

    private static JobApplication CreateApplication()
    {
        return JobApplication.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "Backend developer",
            ApplicationKind.JobPosting,
            ApplicationStatus.InProcess,
            new DateOnly(2026, 9, 1),
            null,
            null);
    }
}
