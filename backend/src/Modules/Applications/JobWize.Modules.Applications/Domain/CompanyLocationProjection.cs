using JobWize.Shared.Domain;

namespace JobWize.Modules.Applications.Domain;

public sealed class CompanyLocationProjection : Entity
{
    public Guid CompanyId { get; private set; }
    public string Label { get; private set; } = default!;
    public bool IsActive { get; private set; }

    private CompanyLocationProjection()
    {
    }

    public static CompanyLocationProjection Create(Guid id, Guid companyId, string label)
    {
        return new CompanyLocationProjection
        {
            Id = id,
            CompanyId = companyId,
            Label = label,
            IsActive = true
        };
    }

    public void Update(string label, bool isActive)
    {
        Label = label;
        IsActive = isActive;
    }
}
