using JobWize.Shared.Domain;

namespace JobWize.Modules.Files.Domain;

public sealed class FileAsset : DomainModel
{
    public Guid CandidateId { get; private set; }
    public FileAssetKind Kind { get; private set; }
    public string FileName { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public long SizeBytes { get; private set; }
    public string StorageKey { get; private set; } = default!;
    public DateTime UploadedAt { get; private set; }
    public DateTime? ArchivedAt { get; private set; }
    public bool IsArchived => ArchivedAt is not null;
    private readonly List<FileBinding> _bindings = [];
    public IReadOnlyCollection<FileBinding> Bindings => _bindings.AsReadOnly();
    public bool HasActiveBindings => _bindings.Any(binding => binding.IsActive);

    private FileAsset()
    {
    }

    public static FileAsset Create(
        Guid id,
        Guid candidateId,
        FileAssetKind kind,
        string fileName,
        string contentType,
        long sizeBytes,
        string storageKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);

        if (sizeBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeBytes));
        }

        return new FileAsset
        {
            Id = id,
            CandidateId = candidateId,
            Kind = kind,
            FileName = fileName.Trim(),
            ContentType = contentType,
            SizeBytes = sizeBytes,
            StorageKey = storageKey,
            UploadedAt = DateTime.UtcNow
        };
    }

    public void Archive(DateTime archivedAt)
    {
        if (IsArchived)
        {
            throw new InvalidOperationException("The document is already archived.");
        }

        ArchivedAt = archivedAt;
    }

    public void BindTo(
        string resourceType,
        Guid resourceId,
        string usage,
        FileBindingAccessPolicy accessPolicy)
    {
        if (IsArchived)
        {
            throw new InvalidOperationException("An archived file cannot be bound to a resource.");
        }

        if (_bindings.Any(binding => binding.IsActive && binding.ResourceType == resourceType && binding.ResourceId == resourceId && binding.Usage == usage))
        {
            return;
        }

        _bindings.Add(FileBinding.Create(Id, resourceType, resourceId, usage, accessPolicy));
    }
}
