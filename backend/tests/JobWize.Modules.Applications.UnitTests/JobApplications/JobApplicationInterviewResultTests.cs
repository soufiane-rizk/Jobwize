using FluentAssertions;
using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using JobWize.Modules.Applications.Contracts.Public.Interviews;
using JobWize.Modules.Applications.Domain;

namespace JobWize.Modules.Applications.UnitTests.JobApplications;

public sealed class JobApplicationInterviewResultTests
{
    [Fact]
    public void RecordInterviewResult_Should_Create_A_Scheduled_Replacement_When_Postponed()
    {
        JobApplication application = JobApplication.Create(
            Guid.NewGuid(), Guid.NewGuid(), null, "Developer",
            ApplicationKind.JobPosting,
            ApplicationStatus.InProcess,
            new DateOnly(2026, 8, 27), null, null);
        JobInterview interview = application.ScheduleInterview(
            InterviewType.Technical, new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc),
            60, InterviewFormat.Video, null, null, []);

        JobInterview? replacement = application.RecordInterviewResult(
            interview.Id, InterviewState.Postponed,
            new DateTime(2026, 9, 2, 10, 0, 0, DateTimeKind.Utc), null);

        replacement.Should().NotBeNull();
        replacement!.State.Should().Be(InterviewState.Scheduled);
        application.Interviews.Should().HaveCount(2);
        interview.State.Should().Be(InterviewState.Postponed);
    }

    [Fact]
    public void RecordInterviewResult_Should_Preserve_Participant_Snapshots_When_Postponed()
    {
        Guid companyContactId = Guid.NewGuid();
        Guid companyLocationId = Guid.NewGuid();
        JobApplication application = JobApplication.Create(
            Guid.NewGuid(), Guid.NewGuid(), companyLocationId, "Developer",
            ApplicationKind.JobPosting,
            ApplicationStatus.InProcess,
            new DateOnly(2026, 8, 27), null, null);
        JobInterview interview = application.ScheduleInterview(
            InterviewType.Technical,
            new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc),
            60,
            InterviewFormat.Video,
            null,
            null,
            [new InterviewParticipantSnapshot(
                companyContactId,
                companyLocationId,
                "Casablanca",
                "Jane Doe",
                "Recruiter",
                "jane@example.com",
                "+212600000000")]);

        JobInterview replacement = application.RecordInterviewResult(
            interview.Id,
            InterviewState.Postponed,
            new DateTime(2026, 9, 2, 10, 0, 0, DateTimeKind.Utc),
            null)!;

        replacement.Participants.Should().ContainSingle().Which.Should().Match<JobInterviewParticipant>(participant =>
            participant.CompanyContactId == companyContactId &&
            participant.CompanyLocationId == companyLocationId &&
            participant.CompanyLocationLabel == "Casablanca" &&
            participant.Name == "Jane Doe" &&
            participant.RoleTitle == "Recruiter" &&
            participant.Email == "jane@example.com" &&
            participant.PhoneNumber == "+212600000000");
    }
}
