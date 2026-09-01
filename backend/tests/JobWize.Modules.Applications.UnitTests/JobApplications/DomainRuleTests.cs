using FluentAssertions;
using JobWize.Modules.Applications.Contracts.Public.Interviews;
using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using JobWize.Modules.Applications.Contracts.Public.Reminders;
using JobWize.Modules.Applications.Domain;
using JobWize.Shared.Errors;

namespace JobWize.Modules.Applications.UnitTests.JobApplications;

public sealed class DomainRuleTests
{
    [Fact]
    public void Create_Should_Require_Applied_Date_For_A_Sent_Application()
    {
        Action action = () => JobApplication.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "Developer",
            ApplicationKind.JobPosting,
            ApplicationStatus.Applied,
            null,
            null,
            null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.AppliedOnRequired);
    }

    [Fact]
    public void ChangeStatus_Should_Reject_An_Unchanged_Status()
    {
        JobApplication application = CreateApplication(ApplicationStatus.Draft, null);

        Action action = () => application.ChangeStatus(ApplicationStatus.Draft, null, null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.ApplicationStatusUnchanged);
    }

    [Fact]
    public void ChangeStatus_Should_Require_Applied_Date_When_Moving_To_Applied()
    {
        JobApplication application = CreateApplication(ApplicationStatus.Planned, null);

        Action action = () => application.ChangeStatus(ApplicationStatus.Applied, null, null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.AppliedOnRequired);
    }

    [Fact]
    public void AddNote_Should_Require_NonEmpty_Text()
    {
        JobApplication application = CreateApplication(ApplicationStatus.InProcess, DateOnly.FromDateTime(DateTime.UtcNow));

        Action action = () => application.AddNote(" ");

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.NoteRequired);
    }

    [Fact]
    public void RecordCvSubmission_Should_Require_At_Least_One_Document()
    {
        JobApplication application = CreateApplication(ApplicationStatus.InProcess, DateOnly.FromDateTime(DateTime.UtcNow));

        Action action = () => application.RecordCvSubmission(
            DateTime.UtcNow,
            CvSubmissionMethod.Email,
            null,
            (null, null, null, null, null, null),
            []);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.CvSubmissionDocumentRequired);
    }

    [Fact]
    public void ScheduleInterview_Should_Require_An_Applied_Application()
    {
        JobApplication application = CreateApplication(ApplicationStatus.Draft, null);

        Action action = () => Schedule(application);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.ApplicationMustBeSentBeforeInterview);
    }

    [Fact]
    public void ScheduleInterview_Should_Reject_Closed_Application_Statuses()
    {
        JobApplication application = CreateApplication(
            ApplicationStatus.Rejected,
            DateOnly.FromDateTime(DateTime.UtcNow));

        Action action = () => Schedule(application);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.CannotScheduleInterviewForCurrentStatus);
    }

    [Fact]
    public void ScheduleInterview_Should_Require_A_Date()
    {
        JobApplication application = CreateApplication(
            ApplicationStatus.InProcess,
            DateOnly.FromDateTime(DateTime.UtcNow));

        Action action = () => application.ScheduleInterview(
            InterviewType.Technical,
            default,
            60,
            InterviewFormat.Video,
            null,
            null,
            []);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.InterviewDateRequired);
    }

    [Fact]
    public void ScheduleInterview_Should_Require_A_Positive_Duration()
    {
        JobApplication application = CreateApplication(
            ApplicationStatus.InProcess,
            DateOnly.FromDateTime(DateTime.UtcNow));

        Action action = () => application.ScheduleInterview(
            InterviewType.Technical,
            DateTime.UtcNow.AddDays(1),
            0,
            InterviewFormat.Video,
            null,
            null,
            []);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.InterviewDurationMustBePositive);
    }

    [Fact]
    public void ScheduleInterview_Should_Require_A_Participant_Name()
    {
        JobApplication application = CreateApplication(
            ApplicationStatus.InProcess,
            DateOnly.FromDateTime(DateTime.UtcNow));

        Action action = () => Schedule(
            application,
            [new InterviewParticipantSnapshot(null, null, null, " ", null, null, null)]);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.InterviewParticipantNameRequired);
    }

    [Fact]
    public void UpdateInterview_Should_Reject_A_Completed_Interview()
    {
        JobApplication application = CreateApplication(
            ApplicationStatus.InProcess,
            DateOnly.FromDateTime(DateTime.UtcNow));
        JobInterview interview = Schedule(application);
        application.RecordInterviewResult(interview.Id, InterviewState.Completed, null, null);

        Action action = () => interview.Update(
            InterviewType.Technical,
            DateTime.UtcNow.AddDays(1),
            60,
            InterviewFormat.Video,
            null,
            null,
            []);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.InterviewCannotBeUpdated);
    }

    [Fact]
    public void RecordInterviewResult_Should_Require_An_Interview_From_The_Application()
    {
        JobApplication application = CreateApplication(
            ApplicationStatus.InProcess,
            DateOnly.FromDateTime(DateTime.UtcNow));

        Action action = () => application.RecordInterviewResult(
            Guid.NewGuid(),
            InterviewState.Completed,
            null,
            null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.InterviewNotInApplication);
    }

    [Fact]
    public void RecordInterviewResult_Should_Require_A_Replacement_Date_When_Postponed()
    {
        JobApplication application = CreateApplication(
            ApplicationStatus.InProcess,
            DateOnly.FromDateTime(DateTime.UtcNow));
        JobInterview interview = Schedule(application);

        Action action = () => application.RecordInterviewResult(
            interview.Id,
            InterviewState.Postponed,
            null,
            null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.InterviewRescheduleDateRequired);
    }

    [Fact]
    public void RecordInterviewResult_Should_Reject_A_Scheduled_Result()
    {
        JobApplication application = CreateApplication(
            ApplicationStatus.InProcess,
            DateOnly.FromDateTime(DateTime.UtcNow));
        JobInterview interview = Schedule(application);

        Action action = () => application.RecordInterviewResult(
            interview.Id,
            InterviewState.Scheduled,
            null,
            null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.InterviewResultMustBeFinal);
    }

    [Fact]
    public void RecordInterviewResult_Should_Reject_A_Result_After_The_Interview_Is_Closed()
    {
        JobApplication application = CreateApplication(
            ApplicationStatus.InProcess,
            DateOnly.FromDateTime(DateTime.UtcNow));
        JobInterview interview = Schedule(application);
        application.RecordInterviewResult(interview.Id, InterviewState.Completed, null, null);

        Action action = () => application.RecordInterviewResult(
            interview.Id,
            InterviewState.Cancelled,
            null,
            null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.InterviewCannotHaveResult);
    }

    [Fact]
    public void CreateReminder_Should_Require_An_Interview_From_The_Application()
    {
        JobApplication application = CreateApplication(
            ApplicationStatus.InProcess,
            DateOnly.FromDateTime(DateTime.UtcNow));

        Action action = () => application.CreateReminder(
            ReminderKind.Interview,
            null,
            Guid.NewGuid(),
            "Prepare",
            DateTime.UtcNow.AddDays(1),
            null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.InterviewNotInApplication);
    }

    [Fact]
    public void CreateReminder_Should_Require_A_Title()
    {
        JobApplication application = CreateApplication(ApplicationStatus.InProcess, DateOnly.FromDateTime(DateTime.UtcNow));

        Action action = () => application.CreateReminder(
            ReminderKind.Custom,
            null,
            null,
            " ",
            DateTime.UtcNow.AddDays(1),
            null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.ReminderTitleRequired);
    }

    [Fact]
    public void CreateReminder_Should_Require_A_Due_Date()
    {
        JobApplication application = CreateApplication(ApplicationStatus.InProcess, DateOnly.FromDateTime(DateTime.UtcNow));

        Action action = () => application.CreateReminder(
            ReminderKind.Custom,
            null,
            null,
            "Follow up",
            default,
            null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.ReminderDueAtRequired);
    }

    [Fact]
    public void CreateReminder_Should_Reject_An_Invalid_Relation()
    {
        JobApplication application = CreateApplication(ApplicationStatus.InProcess, DateOnly.FromDateTime(DateTime.UtcNow));
        JobApplicationCvSubmission submission = application.RecordCvSubmission(
            DateTime.UtcNow,
            CvSubmissionMethod.Email,
            null,
            (null, null, null, null, null, null),
            [(Guid.NewGuid(), "cv.pdf", "application/pdf", 100)]);

        Action action = () => application.CreateReminder(
            ReminderKind.Custom,
            submission.Id,
            null,
            "Follow up",
            DateTime.UtcNow.AddDays(1),
            null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.ReminderRelationInvalid);
    }

    [Fact]
    public void ChangeReminderState_Should_Reject_An_Invalid_State()
    {
        JobApplication application = CreateApplication(ApplicationStatus.InProcess, DateOnly.FromDateTime(DateTime.UtcNow));
        JobApplicationReminder reminder = application.CreateReminder(
            ReminderKind.Custom,
            null,
            null,
            "Follow up",
            DateTime.UtcNow.AddDays(1),
            null);

        Action action = () => application.ChangeReminderState(reminder.Id, (ReminderState)999);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.ReminderStateInvalid);
    }

    private static JobApplication CreateApplication(ApplicationStatus status, DateOnly? appliedOn)
    {
        return JobApplication.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "Developer",
            ApplicationKind.JobPosting,
            status,
            appliedOn,
            null,
            null);
    }

    private static JobInterview Schedule(
        JobApplication application,
        IEnumerable<InterviewParticipantSnapshot>? participants = null)
    {
        return application.ScheduleInterview(
            InterviewType.Technical,
            DateTime.UtcNow.AddDays(1),
            60,
            InterviewFormat.Video,
            null,
            null,
            participants ?? []);
    }
}
