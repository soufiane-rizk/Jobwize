using FluentAssertions;
using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using JobWize.Modules.Applications.Domain;
using JobWize.Shared.Errors;

namespace JobWize.Modules.Applications.UnitTests.JobApplications;

public sealed class JobApplicationStatusTransitionTests
{
    [Fact]
    public void ChangeStatus_Should_Allow_Applied_To_InProcess()
    {
        JobApplication application = Create(ApplicationStatus.Applied);

        application.ChangeStatus(ApplicationStatus.InProcess, null, null);

        application.Status.Should().Be(ApplicationStatus.InProcess);
    }

    [Fact]
    public void ChangeStatus_Should_Reject_InProcess_To_Draft()
    {
        JobApplication application = Create(ApplicationStatus.InProcess);

        Action action = () => application.ChangeStatus(ApplicationStatus.Draft, null, null);

        action.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.ApplicationStatusTransitionNotAllowed);
    }

    [Fact]
    public void ChangeStatus_Should_Allow_Declined_To_Archived_Only()
    {
        JobApplication application = Create(ApplicationStatus.Declined);

        Action rejected = () => application.ChangeStatus(ApplicationStatus.InProcess, null, null);
        rejected.Should().Throw<BusinessRuleException>()
            .Which.Error.Should().Be(DomainErrors.ApplicationStatusTransitionNotAllowed);

        application.ChangeStatus(ApplicationStatus.Archived, null, null);
        application.Status.Should().Be(ApplicationStatus.Archived);
    }

    private static JobApplication Create(ApplicationStatus status)
    {
        return JobApplication.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "Developer",
            ApplicationKind.JobPosting,
            status,
            new DateOnly(2026, 8, 27),
            null,
            null);
    }
}
