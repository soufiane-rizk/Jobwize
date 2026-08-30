using Microsoft.Extensions.Options;

namespace JobWize.Modules.Files.Storage;

internal sealed class LocalFileStorage(IOptions<FileStorageOptions> options) : IFileStorage
{
    private readonly string _rootPath = Path.GetFullPath(options.Value.LocalPath);

    public async Task StoreAsync(string storageKey, Stream content, CancellationToken cancellationToken = default)
    {
        string path = GetPath(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using FileStream output = new(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(output, cancellationToken);
    }

    public Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        string path = GetPath(storageKey);
        Stream? stream = File.Exists(path)
            ? File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)
            : null;

        return Task.FromResult(stream);
    }

    private string GetPath(string storageKey)
    {
        string path = Path.GetFullPath(Path.Combine(_rootPath, storageKey));
        if (!path.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The document storage key is invalid.");
        }

        return path;
    }
}
