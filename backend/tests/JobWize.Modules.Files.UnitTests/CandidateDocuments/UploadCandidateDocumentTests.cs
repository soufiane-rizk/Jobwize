using FluentAssertions;
using JobWize.Modules.Files.Application;
using JobWize.Modules.Files.Application.FileAssets;
using JobWize.Modules.Files.Domain;
using JobWize.Modules.Files.Persistence;
using JobWize.Modules.Files.Storage;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;
using JobWize.Shared.Runtime.Contracts;

namespace JobWize.Modules.Files.UnitTests.FileAssets;

public sealed class UploadFileAssetTests
{
    [Fact]
    public async Task HandleAsync_Should_Store_Pdf_Save_Metadata_And_Publish_Event()
    {
        var repository = new FakeRepository();
        var storage = new FakeStorage();
        var dispatcher = new FakeDispatcher();
        var candidateId = Guid.NewGuid();
        var handler = new UploadFileAsset.Handler(repository, storage, new FakeUserContext(candidateId), dispatcher);

        Result<JobWize.Modules.Files.Contracts.Public.FileAssets.UploadFileAsset.Response> result =
            await handler.HandleAsync(new UploadFileAsset.Command(
                "cv.pdf",
                "application/pdf",
                "%PDF-1.7 test"u8.ToArray()),
                CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repository.SavedDocument.Should().NotBeNull();
        repository.SavedDocument!.CandidateId.Should().Be(candidateId);
        storage.StoredContent.Should().Equal("%PDF-1.7 test"u8.ToArray());
        dispatcher.Published.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleAsync_Should_Reject_A_File_With_A_Forged_Extension()
    {
        var handler = new UploadFileAsset.Handler(
            new FakeRepository(),
            new FakeStorage(),
            new FakeUserContext(Guid.NewGuid()),
            new FakeDispatcher());

        Result<JobWize.Modules.Files.Contracts.Public.FileAssets.UploadFileAsset.Response> result =
            await handler.HandleAsync(new UploadFileAsset.Command(
                "cv.pdf",
                "application/pdf",
                "not a pdf"u8.ToArray()),
                CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FilesErrors.InvalidFile);
    }

    private sealed class FakeRepository : IFileAssetRepository
    {
        public FileAsset? SavedDocument { get; private set; }
        public Task<FileAsset?> GetByIdAsync(Guid documentId, Guid candidateId, CancellationToken cancellationToken = default) =>
            Task.FromResult<FileAsset?>(null);
        public Task SaveAsync(FileAsset document, CancellationToken cancellationToken = default)
        {
            SavedDocument = document;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeStorage : IFileStorage
    {
        public byte[]? StoredContent { get; private set; }
        public async Task StoreAsync(string storageKey, Stream content, CancellationToken cancellationToken = default)
        {
            using var memory = new MemoryStream();
            await content.CopyToAsync(memory, cancellationToken);
            StoredContent = memory.ToArray();
        }
        public Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default) =>
            Task.FromResult<Stream?>(null);
    }

    private sealed class FakeUserContext(Guid userId) : IUserContext
    {
        public Guid UserId { get; } = userId;
    }

    private sealed class FakeDispatcher : IDispatcher
    {
        public INotification? Published { get; private set; }
        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<TResponse> SendModuleQueryAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task PublishAsync(INotification notification, CancellationToken cancellationToken = default)
        {
            Published = notification;
            return Task.CompletedTask;
        }
    }
}
