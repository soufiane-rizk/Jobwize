namespace JobWize.Modules.Files.Storage;

public sealed class FileStorageOptions
{
    public const string SectionName = "Files:Storage";
    public string LocalPath { get; init; } = "App_Data/files";
}
