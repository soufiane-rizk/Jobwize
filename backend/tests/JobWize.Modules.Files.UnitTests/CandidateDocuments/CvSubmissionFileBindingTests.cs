using FluentAssertions;
using JobWize.Modules.Applications.Contracts.Events.JobApplications;
using JobWize.Modules.Files.Application.FileAssets;
using JobWize.Modules.Files.Domain;
using JobWize.Modules.Files.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Files.UnitTests.FileAssets;

public sealed class CvSubmissionFileBindingTests
{
    [Fact]
    public async Task HandleAsync_Should_Bind_Each_Submitted_Document_Once()
    {
        Guid candidateId = Guid.NewGuid();
        Guid submissionId = Guid.NewGuid();
        FileAsset file = CreateFile(candidateId);
        var repository = new FakeRepository(file);
        var handler = new BindFilesToCvSubmission(repository);
        var notification = new JobApplicationCvSubmitted(
            submissionId,
            Guid.NewGuid(),
            candidateId,
            [file.Id, file.Id]);

        await handler.HandleAsync(notification, CancellationToken.None);
        await handler.HandleAsync(notification, CancellationToken.None);

        file.Bindings.Should().ContainSingle(binding =>
            binding.ResourceType == "JobApplicationCvSubmission" &&
            binding.ResourceId == submissionId &&
            binding.Usage == "SubmittedDocument" &&
            binding.AccessPolicy == FileBindingAccessPolicy.OwnerOnly);
    }

    [Fact]
    public async Task BindTo_Should_Track_A_New_Binding_As_Added()
    {
        Guid candidateId = Guid.NewGuid();
        FileAsset file = CreateFile(candidateId);
        DbContextOptions<FilesDbContext> options = new DbContextOptionsBuilder<FilesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new FilesDbContext(options);
        dbContext.FileAssets.Add(file);
        await dbContext.SaveChangesAsync();

        file.BindTo(
            "JobApplicationCvSubmission",
            Guid.NewGuid(),
            "SubmittedDocument",
            FileBindingAccessPolicy.OwnerOnly);
        FileBinding binding = file.Bindings.Single();
        dbContext.ChangeTracker.DetectChanges();

        dbContext.Entry(binding).State.Should().Be(EntityState.Added);
    }

    private static FileAsset CreateFile(Guid candidateId)
    {
        return FileAsset.Create(
            Guid.NewGuid(),
            candidateId,
            FileAssetKind.CandidateDocument,
            "cv.pdf",
            "application/pdf",
            100,
            "candidate/cv.pdf");
    }

    private sealed class FakeRepository(FileAsset file) : IFileAssetRepository
    {
        public Task<FileAsset?> GetByIdAsync(
            Guid documentId,
            Guid candidateId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<FileAsset?>(
                file.Id == documentId && file.CandidateId == candidateId
                    ? file
                    : null);
        }

        public Task SaveAsync(
            FileAsset document,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
