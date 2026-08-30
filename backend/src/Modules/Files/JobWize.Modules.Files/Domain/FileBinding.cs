namespace JobWize.Modules.Files.Domain;

public sealed class FileBinding
{
    public Guid Id { get; private set; }
    public Guid FileAssetId { get; private set; }
    public string ResourceType { get; private set; } = default!;
    public Guid ResourceId { get; private set; }
    public string Usage { get; private set; } = default!;
    public FileBindingAccessPolicy AccessPolicy { get; private set; }
    public DateTime BoundAt { get; private set; }
    public DateTime? ReleasedAt { get; private set; }
    public bool IsActive => ReleasedAt is null;

    private FileBinding()
    {
    }

    internal static FileBinding Create(
        Guid fileAssetId,
        string resourceType,
        Guid resourceId,
        string usage,
        FileBindingAccessPolicy accessPolicy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceType);
        ArgumentException.ThrowIfNullOrWhiteSpace(usage);

        return new FileBinding
        {
            Id = Guid.NewGuid(),
            FileAssetId = fileAssetId,
            ResourceType = resourceType.Trim(),
            ResourceId = resourceId,
            Usage = usage.Trim(),
            AccessPolicy = accessPolicy,
            BoundAt = DateTime.UtcNow
        };
    }
}
