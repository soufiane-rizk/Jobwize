using JobWize.Modules.Companies.Contracts.Public.Companies;
using JobWize.Shared.Domain;

namespace JobWize.Modules.Applications.Domain;

public sealed class CompanyProjection : Entity
{
    public string Name { get; private set; } = default!;
    public CompanyVisibility Visibility { get; private set; }
    public Guid? CreatedByCandidateId { get; private set; }
    public bool IsActive { get; private set; }
    private readonly List<CompanyLocationProjection> _locations = [];
    public IReadOnlyCollection<CompanyLocationProjection> Locations => _locations.AsReadOnly();

    private CompanyProjection()
    {
    }

    public static CompanyProjection CreateOrUpdate(
        Guid id,
        string name,
        CompanyVisibility visibility,
        Guid? createdByCandidateId,
        bool isActive)
    {
        return new CompanyProjection
        {
            Id = id,
            Name = name,
            Visibility = visibility,
            CreatedByCandidateId = createdByCandidateId,
            IsActive = isActive
        };
    }

    public void Update(string name, CompanyVisibility visibility, Guid? createdByCandidateId, bool isActive)
    {
        Name = name;
        Visibility = visibility;
        CreatedByCandidateId = createdByCandidateId;
        IsActive = isActive;
    }

    public void Deactivate()
    {
        IsActive = false;
        SynchronizeLocations([]);
    }

    public void SynchronizeLocations(IEnumerable<(Guid Id, string Label)> locations)
    {
        Guid[] sourceLocationIds = locations.Select(location => location.Id).ToArray();

        foreach (CompanyLocationProjection existing in _locations.Where(location => !sourceLocationIds.Contains(location.Id)))
        {
            existing.Update(existing.Label, false);
        }

        foreach ((Guid id, string label) in locations)
        {
            CompanyLocationProjection? existing = _locations.SingleOrDefault(location => location.Id == id);
            if (existing is null)
            {
                _locations.Add(CompanyLocationProjection.Create(id, Id, label));
            }
            else
            {
                existing.Update(label, true);
            }
        }
    }
}
