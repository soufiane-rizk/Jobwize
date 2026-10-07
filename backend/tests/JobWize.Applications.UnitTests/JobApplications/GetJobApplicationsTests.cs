using FluentAssertions;
using JobWize.Modules.Applications.Application.JobApplications;
using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using JobWize.Modules.Applications.Domain;
using JobWize.Modules.Applications.Persistence;
using JobWize.Shared.Application.Security;
using Microsoft.EntityFrameworkCore;
using GetJobApplicationsFeature = JobWize.Modules.Applications.Application.JobApplications.GetJobApplications;

namespace JobWize.Applications.UnitTests.JobApplications;

public sealed class GetJobApplicationsTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Only_Applications_Belonging_To_Current_Candidate()
    {
        var candidateId = Guid.NewGuid();
        var anotherCandidateId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<ApplicationsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new ApplicationsDbContext(options);

        dbContext.JobApplications.AddRange(
            JobApplication.Create(candidateId, "Acme", "Backend developer", ApplicationKind.JobPosting, ApplicationStatus.Planned, null, null, null),
            JobApplication.Create(anotherCandidateId, "Other", "Frontend developer", ApplicationKind.JobPosting, ApplicationStatus.Planned, null, null, null));

        await dbContext.SaveChangesAsync();

        var handler = new GetJobApplicationsFeature.Handler(
            dbContext,
            new FakeUserContext(candidateId));

        var result = await handler.HandleAsync(
            new GetJobApplicationsFeature.Query(),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Applications.Should().ContainSingle();
        result.Value.Applications[0].CompanyName.Should().Be("Acme");
    }

    [Fact]
    public async Task ChangeStatus_Should_Add_A_New_Status_History_Record()
    {
        var options = new DbContextOptionsBuilder<ApplicationsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new ApplicationsDbContext(options);

        JobApplication application = JobApplication.Create(
            Guid.NewGuid(),
            "Acme",
            "Backend developer",
            ApplicationKind.JobPosting,
            ApplicationStatus.Planned,
            null,
            null,
            null);

        dbContext.JobApplications.Add(application);
        await dbContext.SaveChangesAsync();

        application.ChangeStatus(
            ApplicationStatus.Applied,
            new DateOnly(2026, 8, 27),
            "CV sent.");

        dbContext.ChangeTracker.DetectChanges();

        dbContext.Entry(application.Activities.Last()).State.Should().Be(EntityState.Added);

        await dbContext.SaveChangesAsync();

        dbContext.Entry(application.Activities.Last()).State.Should().Be(EntityState.Unchanged);
    }

    private sealed class FakeUserContext(Guid userId) : IUserContext
    {
        public Guid UserId { get; } = userId;
    }
}
