using JobWize.Shared.Errors;

namespace JobWize.Modules.Files.Domain;

public static class DomainErrors
{
    public static readonly Error FileNameRequired = new(
        "Files.Domain.FileNameRequired",
        "A file name is required.",
        ErrorType.Validation);

    public static readonly Error ContentTypeRequired = new(
        "Files.Domain.ContentTypeRequired",
        "A file content type is required.",
        ErrorType.Validation);

    public static readonly Error StorageKeyRequired = new(
        "Files.Domain.StorageKeyRequired",
        "A file storage key is required.",
        ErrorType.Validation);

    public static readonly Error FileSizeMustBePositive = new(
        "Files.Domain.FileSizeMustBePositive",
        "A file size must be greater than zero.",
        ErrorType.Validation);

    public static readonly Error BindingResourceTypeRequired = new(
        "Files.Domain.BindingResourceTypeRequired",
        "A binding resource type is required.",
        ErrorType.Validation);

    public static readonly Error BindingUsageRequired = new(
        "Files.Domain.BindingUsageRequired",
        "A binding usage is required.",
        ErrorType.Validation);

    public static readonly Error FileAlreadyArchived = new(
        "Files.Domain.FileAlreadyArchived",
        "The file is already archived.",
        ErrorType.Validation);

    public static readonly Error ArchivedFileCannotBeBound = new(
        "Files.Domain.ArchivedFileCannotBeBound",
        "An archived file cannot be bound to a resource.",
        ErrorType.Validation);
}
