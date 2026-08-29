using JobWize.Modules.Companies.Contracts.Public.CompanyContacts;
using JobWize.Shared.Domain;

namespace JobWize.Modules.Companies.Domain;

public sealed class CompanyContact : Entity
{
    public Guid CompanyId { get; private set; }
    public Guid? CompanyLocationId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? RoleTitle { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public CompanyContactVisibility Visibility { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid? CreatedByCandidateId { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? ReviewReason { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private CompanyContact()
    {
    }

    internal static CompanyContact CreatePrivate(
        Guid companyId,
        Guid? companyLocationId,
        Guid candidateId,
        string name,
        string? roleTitle,
        string? email,
        string? phoneNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new CompanyContact
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            CompanyLocationId = companyLocationId,
            Name = name.Trim(),
            RoleTitle = Normalize(roleTitle),
            Email = Normalize(email),
            PhoneNumber = Normalize(phoneNumber),
            Visibility = CompanyContactVisibility.Private,
            CreatedByCandidateId = candidateId
        };
    }

    internal static CompanyContact CreateShared(
        Guid companyId,
        Guid? companyLocationId,
        string name,
        string? roleTitle,
        string? email,
        string? phoneNumber)
    {
        CompanyContact contact = CreatePrivate(
            companyId,
            companyLocationId,
            Guid.Empty,
            name,
            roleTitle,
            email,
            phoneNumber);

        contact.CreatedByCandidateId = null;
        contact.Visibility = CompanyContactVisibility.Shared;

        return contact;
    }

    internal void UpdateInformation(
        Guid? companyLocationId,
        string name,
        string? roleTitle,
        string? email,
        string? phoneNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        CompanyLocationId = companyLocationId;
        Name = name.Trim();
        RoleTitle = Normalize(roleTitle);
        Email = Normalize(email);
        PhoneNumber = Normalize(phoneNumber);
    }

    internal void Approve(Guid reviewerId, DateTime reviewedAt, string? reason)
    {
        if (Visibility == CompanyContactVisibility.Shared)
        {
            throw new InvalidOperationException("A shared company contact cannot be reviewed again.");
        }

        Visibility = CompanyContactVisibility.Shared;
        ReviewedByUserId = reviewerId;
        ReviewedAt = reviewedAt;
        ReviewReason = Normalize(reason);
    }

    internal void Reject(Guid reviewerId, DateTime reviewedAt, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (Visibility == CompanyContactVisibility.Shared)
        {
            throw new InvalidOperationException("A shared company contact cannot be reviewed again.");
        }

        Visibility = CompanyContactVisibility.Private;
        ReviewedByUserId = reviewerId;
        ReviewedAt = reviewedAt;
        ReviewReason = reason.Trim();
    }

    internal void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
