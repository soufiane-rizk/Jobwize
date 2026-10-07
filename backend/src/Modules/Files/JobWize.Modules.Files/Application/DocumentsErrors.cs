using JobWize.Shared.Application.Results;

namespace JobWize.Modules.Files.Application;

public static class FilesErrors
{
    public static readonly Error DocumentNotFound = new(
        "Files.DocumentNotFound",
        "The requested document was not found.",
        ErrorType.NotFound);

    public static readonly Error InvalidFile = new(
        "Files.InvalidFile",
        "Upload a PDF, DOC, or DOCX file no larger than 10 MB.",
        ErrorType.Validation);
}
