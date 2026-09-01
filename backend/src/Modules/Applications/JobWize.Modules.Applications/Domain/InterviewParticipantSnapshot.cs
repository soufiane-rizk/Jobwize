namespace JobWize.Modules.Applications.Domain;

public sealed record InterviewParticipantSnapshot(
    Guid? CompanyContactId,
    Guid? CompanyLocationId,
    string? CompanyLocationLabel,
    string Name,
    string? RoleTitle,
    string? Email,
    string? PhoneNumber);
