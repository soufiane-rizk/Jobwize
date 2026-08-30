using JobWize.Modules.Companies.Contracts.Public.Companies;
using JobWize.Shared.Domain;

namespace JobWize.Modules.Companies.Domain;

public sealed class Company : DomainModel
{
    public string Name { get; private set; } = default!;
    public string? Website { get; private set; }
    public string? Industry { get; private set; }
    public string? Description { get; private set; }
    public CompanyVisibility Visibility { get; private set; }
    public Guid? CreatedByCandidateId { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? ReviewReason { get; private set; }

    private readonly List<CompanyLocation> _locations = [];
    public IReadOnlyCollection<CompanyLocation> Locations => _locations.AsReadOnly();

    private Company()
    {
    }

    public static Company CreatePrivate(
        Guid candidateId,
        string name,
        string? website,
        string? industry,
        string? description,
        IEnumerable<(string Label, string City, string Country, string? Address)> locations)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Website = Normalize(website),
            Industry = Normalize(industry),
            Description = Normalize(description),
            Visibility = CompanyVisibility.Private,
            CreatedByCandidateId = candidateId
        };

        foreach ((string label, string city, string country, string? address) in locations)
        {
            company.AddLocation(label, city, country, address);
        }

        return company;
    }

    public static Company CreateShared(
        string name,
        string? website,
        string? industry,
        string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Company
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Website = Normalize(website),
            Industry = Normalize(industry),
            Description = Normalize(description),
            Visibility = CompanyVisibility.Shared
        };
    }

    public void AddLocation(string label, string city, string country, string? address)
    {
        _locations.Add(CompanyLocation.Create(Id, label, city, country, Normalize(address)));
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
