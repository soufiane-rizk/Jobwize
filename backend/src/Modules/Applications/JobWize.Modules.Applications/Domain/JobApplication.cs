using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using JobWize.Shared.Domain;

namespace JobWize.Modules.Applications.Domain;

public sealed class JobApplication : DomainModel
{
    public Guid CandidateId { get; private set; }
    public string CompanyName { get; private set; } = default!;
    public string? RoleTitle { get; private set; }
    public ApplicationKind Kind { get; private set; }
    public ApplicationStatus Status { get; private set; }
    public DateOnly? AppliedOn { get; private set; }
    public string? SourceUrl { get; private set; }
    public string? Notes { get; private set; }
    private JobApplication()
    {
    }

    public static JobApplication Create(
        Guid candidateId,
        string companyName,
        string? roleTitle,
        ApplicationKind kind,
        ApplicationStatus status,
        DateOnly? appliedOn,
        string? sourceUrl,
        string? notes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(companyName);

        if (status == ApplicationStatus.Applied && appliedOn is null)
        {
            throw new ArgumentException(
                "Applied on is required when the application has been sent.",
                nameof(appliedOn));
        }

        return new JobApplication
        {
            Id = Guid.NewGuid(),
            CandidateId = candidateId,
            CompanyName = companyName.Trim(),
            RoleTitle = string.IsNullOrWhiteSpace(roleTitle) ? null : roleTitle.Trim(),
            Kind = kind,
            Status = status,
            AppliedOn = appliedOn,
            SourceUrl = string.IsNullOrWhiteSpace(sourceUrl) ? null : sourceUrl.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };
    }
}
