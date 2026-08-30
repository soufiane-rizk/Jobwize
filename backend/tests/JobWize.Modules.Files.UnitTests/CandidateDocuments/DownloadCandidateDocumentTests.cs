using FluentAssertions;
using JobWize.Modules.Files.Application;
using JobWize.Modules.Files.Application.FileAssets;
using JobWize.Modules.Files.Domain;
using JobWize.Modules.Files.Persistence;
using JobWize.Modules.Files.Storage;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;

namespace JobWize.Modules.Files.UnitTests.FileAssets;

public sealed class DownloadCandidateDocumentTests
{
    [Fact]
    public async Task HandleAsync_Should_Allow_Owner_To_Download_Archived_Bound_Document()
    {
        Guid candidateId = Guid.NewGuid();
        FileAsset file = CreateFile(candidateId);
        file.BindTo(
            "JobApplicationCvSubmission",
            Guid.NewGuid(),
            "SubmittedDocument",
            FileBindingAccessPolicy.OwnerOnly);
        file.Archive(DateTime.UtcNow);

        var handler = new DownloadFileAsset.Handler(
            new FakeRepository(file),
            new FakeStorage(),
            new FakeUserContext(candidateId));

        Result<DownloadFileAsset.FileDownload> result = await handler.HandleAsync(
            new DownloadFileAsset.Query(file.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FileName.Should().Be("cv.pdf");
    }

    [Fact]
    public async Task HandleAsync_Should_Reject_Archived_Unbound_Document()
    {
        Guid candidateId = Guid.NewGuid();
        FileAsset file = CreateFile(candidateId);
        file.Archive(DateTime.UtcNow);

        var handler = new DownloadFileAsset.Handler(
            new FakeRepository(file),
            new FakeStorage(),
            new FakeUserContext(candidateId));

        Result<DownloadFileAsset.FileDownload> result = await handler.HandleAsync(
            new DownloadFileAsset.Query(file.Id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FilesErrors.DocumentNotFound);
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

    private sealed class FakeStorage : IFileStorage
    {
        public Task StoreAsync(
            string storageKey,
            Stream content,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Stream?> OpenReadAsync(
            string storageKey,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Stream?>(new MemoryStream("file"u8.ToArray()));
        }
    }

    private sealed class FakeUserContext(Guid userId) : IUserContext
    {
        public Guid UserId { get; } = userId;
    }
}
