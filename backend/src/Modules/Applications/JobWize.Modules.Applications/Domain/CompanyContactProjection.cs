using JobWize.Modules.Companies.Contracts.Public.CompanyContacts;
using JobWize.Shared.Domain;

namespace JobWize.Modules.Applications.Domain;

public sealed class CompanyContactProjection : Entity
{
    public Guid CompanyId { get; private set; }
    public Guid? CompanyLocationId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? RoleTitle { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public CompanyContactVisibility Visibility { get; private set; }
    public Guid? CreatedByCandidateId { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsRejected { get; private set; }

    private CompanyContactProjection()
    {
    }

    public static CompanyContactProjection Create(
        Guid id,
        Guid companyId,
        Guid? companyLocationId,
        string name,
        string? roleTitle,
        string? email,
        string? phoneNumber,
        CompanyContactVisibility visibility,
        Guid? createdByCandidateId,
        bool isActive,
        bool isRejected)
    {
        return new CompanyContactProjection
        {
            Id = id,
            CompanyId = companyId,
            CompanyLocationId = companyLocationId,
            Name = name,
            RoleTitle = roleTitle,
            Email = email,
            PhoneNumber = phoneNumber,
            Visibility = visibility,
            CreatedByCandidateId = createdByCandidateId,
            IsActive = isActive,
            IsRejected = isRejected
        };
    }

    public void Update(
        Guid? companyLocationId,
        string name,
        string? roleTitle,
        string? email,
        string? phoneNumber,
        CompanyContactVisibility visibility,
        Guid? createdByCandidateId,
        bool isActive,
        bool isRejected)
    {
        CompanyLocationId = companyLocationId;
        Name = name;
        RoleTitle = roleTitle;
        Email = email;
        PhoneNumber = phoneNumber;
        Visibility = visibility;
        CreatedByCandidateId = createdByCandidateId;
        IsActive = isActive;
        IsRejected = isRejected;
    }
}
