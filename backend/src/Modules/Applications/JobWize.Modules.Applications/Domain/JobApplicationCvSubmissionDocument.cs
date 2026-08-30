using JobWize.Shared.Domain;

namespace JobWize.Modules.Applications.Domain;

public sealed class JobApplicationCvSubmissionDocument : Entity
{
    public Guid JobApplicationCvSubmissionId { get; private set; }
    public Guid FileId { get; private set; }
    public string FileName { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public long SizeBytes { get; private set; }

    private JobApplicationCvSubmissionDocument()
    {
    }

    internal static JobApplicationCvSubmissionDocument Create(
        Guid submissionId,
        Guid fileId,
        string fileName,
        string contentType,
        long sizeBytes)
    {
        return new JobApplicationCvSubmissionDocument
        {
            Id = Guid.NewGuid(),
            JobApplicationCvSubmissionId = submissionId,
            FileId = fileId,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = sizeBytes
        };
    }
}
