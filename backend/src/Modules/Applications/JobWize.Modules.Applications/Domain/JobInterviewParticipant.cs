using JobWize.Shared.Domain;

namespace JobWize.Modules.Applications.Domain;

public sealed class JobInterviewParticipant : Entity
{
    public Guid JobInterviewId { get; private set; }
    public Guid? CompanyContactId { get; private set; }
    public Guid? CompanyLocationId { get; private set; }
    public string? CompanyLocationLabel { get; private set; }
    public string Name { get; private set; } = default!;
    public string? RoleTitle { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }

    private JobInterviewParticipant()
    {
    }

    internal static JobInterviewParticipant Create(
        Guid interviewId,
        InterviewParticipantSnapshot snapshot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshot.Name);

        return new JobInterviewParticipant
        {
            Id = Guid.NewGuid(),
            JobInterviewId = interviewId,
            CompanyContactId = snapshot.CompanyContactId,
            CompanyLocationId = snapshot.CompanyLocationId,
            CompanyLocationLabel = Normalize(snapshot.CompanyLocationLabel),
            Name = snapshot.Name.Trim(),
            RoleTitle = Normalize(snapshot.RoleTitle),
            Email = Normalize(snapshot.Email),
            PhoneNumber = Normalize(snapshot.PhoneNumber)
        };
    }

    internal InterviewParticipantSnapshot ToSnapshot() => new(
        CompanyContactId,
        CompanyLocationId,
        CompanyLocationLabel,
        Name,
        RoleTitle,
        Email,
        PhoneNumber);

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
