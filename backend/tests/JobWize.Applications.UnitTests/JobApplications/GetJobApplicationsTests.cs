using FluentAssertions;
using JobWize.Modules.Applications.Application.JobApplications;
using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using JobWize.Modules.Applications.Domain;
using JobWize.Modules.Applications.Persistence;
using JobWize.Modules.Companies.Contracts.Public.Companies;
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

        var acmeCompanyId = Guid.NewGuid();
        var otherCompanyId = Guid.NewGuid();

        dbContext.CompanyProjections.AddRange(
            CompanyProjection.CreateOrUpdate(acmeCompanyId, "Acme", CompanyVisibility.Shared, null, true),
            CompanyProjection.CreateOrUpdate(otherCompanyId, "Other", CompanyVisibility.Shared, null, true));

        dbContext.JobApplications.AddRange(
            JobApplication.Create(candidateId, acmeCompanyId, null, "Backend developer", ApplicationKind.JobPosting, ApplicationStatus.Planned, null, null, null),
            JobApplication.Create(anotherCandidateId, otherCompanyId, null, "Frontend developer", ApplicationKind.JobPosting, ApplicationStatus.Planned, null, null, null));

        await dbContext.SaveChangesAsync();

        var handler = new GetJobApplicationsFeature.Handler(
            dbContext,
            new FakeUserContext(candidateId));

        var result = await handler.HandleAsync(
            new GetJobApplicationsFeature.Query(null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Applications.Should().ContainSingle();
        result.Value.Applications[0].CompanyName.Should().Be("Acme");
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Only_Applications_For_The_Requested_Company()
    {
        var candidateId = Guid.NewGuid();
        var requestedCompanyId = Guid.NewGuid();
        var otherCompanyId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<ApplicationsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new ApplicationsDbContext(options);

        dbContext.CompanyProjections.AddRange(
            CompanyProjection.CreateOrUpdate(
                requestedCompanyId,
                "Acme",
                CompanyVisibility.Shared,
                null,
                true),
            CompanyProjection.CreateOrUpdate(
                otherCompanyId,
                "Other",
                CompanyVisibility.Shared,
                null,
                true));

        dbContext.JobApplications.AddRange(
            JobApplication.Create(
                candidateId,
                requestedCompanyId,
                null,
                "Backend developer",
                ApplicationKind.JobPosting,
                ApplicationStatus.Planned,
                null,
                null,
                null),
            JobApplication.Create(
                candidateId,
                otherCompanyId,
                null,
                "Frontend developer",
                ApplicationKind.JobPosting,
                ApplicationStatus.Planned,
                null,
                null,
                null));

        await dbContext.SaveChangesAsync();

        var handler = new GetJobApplicationsFeature.Handler(
            dbContext,
            new FakeUserContext(candidateId));

        var result = await handler.HandleAsync(
            new GetJobApplicationsFeature.Query(requestedCompanyId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Applications.Should().ContainSingle();
        result.Value.Applications[0].CompanyId.Should().Be(requestedCompanyId);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_The_Legacy_Company_Name_When_No_Company_Is_Linked()
    {
        Guid candidateId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<ApplicationsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new ApplicationsDbContext(options);

        JobApplication application = JobApplication.Create(
            candidateId,
            Guid.NewGuid(),
            null,
            "Backend developer",
            ApplicationKind.JobPosting,
            ApplicationStatus.Planned,
            null,
            null,
            null);

        dbContext.JobApplications.Add(application);
        dbContext.Entry(application).Property("CompanyId").CurrentValue = null;
        dbContext.Entry(application).Property("LegacyCompanyName").CurrentValue = "Legacy Acme";
        await dbContext.SaveChangesAsync();

        var handler = new GetJobApplicationsFeature.Handler(
            dbContext,
            new FakeUserContext(candidateId));

        var result = await handler.HandleAsync(
            new GetJobApplicationsFeature.Query(null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Applications.Should().ContainSingle();
        result.Value.Applications[0].CompanyName.Should().Be("Legacy Acme");
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
            Guid.NewGuid(),
            null,
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
