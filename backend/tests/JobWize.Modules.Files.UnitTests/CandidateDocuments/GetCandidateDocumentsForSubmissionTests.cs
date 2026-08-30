using FluentAssertions;
using JobWize.Modules.Files.Application.FileAssets;
using JobWize.Modules.Files.Domain;
using JobWize.Modules.Files.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Files.UnitTests.FileAssets;

public sealed class GetCandidateDocumentsForSubmissionTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Only_Active_Documents_Owned_By_The_Candidate()
    {
        Guid candidateId = Guid.NewGuid();
        FileAsset selectable = CreateFile(candidateId, FileAssetKind.CandidateDocument);
        FileAsset archived = CreateFile(candidateId, FileAssetKind.CandidateDocument);
        FileAsset otherCandidatesFile = CreateFile(Guid.NewGuid(), FileAssetKind.CandidateDocument);
        FileAsset avatar = CreateFile(candidateId, FileAssetKind.UserAvatar);
        archived.Archive(DateTime.UtcNow);

        DbContextOptions<FilesDbContext> options = new DbContextOptionsBuilder<FilesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new FilesDbContext(options);
        dbContext.FileAssets.AddRange(selectable, archived, otherCandidatesFile, avatar);
        await dbContext.SaveChangesAsync();

        var handler = new GetCandidateDocumentsForSubmission.Handler(dbContext);
        JobWize.Modules.Files.Contracts.Internal.FileAssets.GetCandidateDocumentsForSubmission.Response result =
            await handler.HandleAsync(
                new JobWize.Modules.Files.Contracts.Internal.FileAssets.GetCandidateDocumentsForSubmission.Query(
                    candidateId,
                    [selectable.Id, archived.Id, otherCandidatesFile.Id, avatar.Id]),
                CancellationToken.None);

        result.Files.Should().ContainSingle().Which.FileId.Should().Be(selectable.Id);
    }

    private static FileAsset CreateFile(Guid candidateId, FileAssetKind kind)
    {
        return FileAsset.Create(
            Guid.NewGuid(),
            candidateId,
            kind,
            "file.pdf",
            "application/pdf",
            100,
            Guid.NewGuid().ToString());
    }
}
