using JobWize.Shared.Domain;

namespace JobWize.Modules.Companies.Domain;

public sealed class CompanyLocation : Entity
{
    public Guid CompanyId { get; private set; }
    public string Label { get; private set; } = default!;
    public string City { get; private set; } = default!;
    public string Country { get; private set; } = default!;
    public string? Address { get; private set; }

    private CompanyLocation()
    {
    }

    internal static CompanyLocation Create(
        Guid companyId,
        string label,
        string city,
        string country,
        string? address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(country);

        return new CompanyLocation
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Label = label.Trim(),
            City = city.Trim(),
            Country = country.Trim(),
            Address = address
        };
    }
}
