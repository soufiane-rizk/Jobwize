namespace JobWize.Modules.Files.Storage;

public interface IFileStorage
{
    Task StoreAsync(string storageKey, Stream content, CancellationToken cancellationToken = default);
    Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);
}
