using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using JobWize.Shared.Domain;

namespace JobWize.Modules.Applications.Domain;

public sealed class JobApplicationCvSubmission : Entity
{
    public Guid JobApplicationId { get; private set; }
    public DateTime SentAt { get; private set; }
    public CvSubmissionMethod Method { get; private set; }
    public string? Notes { get; private set; }
    public Guid? CompanyContactId { get; private set; }
    public Guid? CompanyLocationId { get; private set; }
    public string? ContactName { get; private set; }
    public string? ContactRoleTitle { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ContactPhoneNumber { get; private set; }
    private readonly List<JobApplicationCvSubmissionDocument> _documents = [];
    public IReadOnlyCollection<JobApplicationCvSubmissionDocument> Documents => _documents.AsReadOnly();

    private JobApplicationCvSubmission()
    {
    }

    internal static JobApplicationCvSubmission Create(
        Guid applicationId,
        DateTime sentAt,
        CvSubmissionMethod method,
        string? notes,
        (Guid? Id, Guid? LocationId, string? Name, string? RoleTitle, string? Email, string? PhoneNumber) contact,
        IEnumerable<(Guid FileId, string FileName, string ContentType, long SizeBytes)> documents)
    {
        JobApplicationCvSubmission submission = new()
        {
            Id = Guid.NewGuid(),
            JobApplicationId = applicationId,
            SentAt = sentAt,
            Method = method,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            CompanyContactId = contact.Id,
            CompanyLocationId = contact.LocationId,
            ContactName = contact.Name,
            ContactRoleTitle = contact.RoleTitle,
            ContactEmail = contact.Email,
            ContactPhoneNumber = contact.PhoneNumber
        };

        foreach ((Guid fileId, string fileName, string contentType, long sizeBytes) in documents)
        {
            submission._documents.Add(JobApplicationCvSubmissionDocument.Create(submission.Id, fileId, fileName, contentType, sizeBytes));
        }

        return submission;
    }
}
