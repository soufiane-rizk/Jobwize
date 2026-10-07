using FluentAssertions;
using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using JobWize.Modules.Applications.Contracts.Public.Interviews;
using JobWize.Modules.Applications.Domain;

namespace JobWize.Applications.UnitTests.JobApplications;

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
}
