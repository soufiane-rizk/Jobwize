using JobWize.Shared.Domain;
using JobWize.Modules.Companies.Contracts.Public.Companies;

namespace JobWize.Modules.Applications.Domain;

public sealed class CompanyLocationProjection : Entity
{
    public Guid CompanyId { get; private set; }
    public string Label { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public CompanyLocationVisibility Visibility { get; private set; }
    public Guid? CreatedByCandidateId { get; private set; }

    private CompanyLocationProjection()
    {
    }

    public static CompanyLocationProjection Create(
        Guid id,
        Guid companyId,
        string label,
        CompanyLocationVisibility visibility,
        Guid? createdByCandidateId,
        bool isActive)
    {
        return new CompanyLocationProjection
        {
            Id = id,
            CompanyId = companyId,
            Label = label,
            IsActive = isActive,
            Visibility = visibility,
            CreatedByCandidateId = createdByCandidateId
        };
    }

    public void Update(
        string label,
        CompanyLocationVisibility visibility,
        Guid? createdByCandidateId,
        bool isActive)
    {
        Label = label;
        IsActive = isActive;
        Visibility = visibility;
        CreatedByCandidateId = createdByCandidateId;
    }
}
