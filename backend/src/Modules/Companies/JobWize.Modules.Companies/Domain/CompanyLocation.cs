using JobWize.Shared.Domain;
using JobWize.Shared.Errors;
using JobWize.Modules.Companies.Contracts.Public.Companies;

namespace JobWize.Modules.Companies.Domain;

public sealed class CompanyLocation : Entity
{
    public Guid CompanyId { get; private set; }
    public string? Label { get; private set; }
    public string City { get; private set; } = default!;
    public string Country { get; private set; } = default!;
    public string? Address { get; private set; }
    public CompanyLocationVisibility Visibility { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid? CreatedByCandidateId { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? ReviewReason { get; private set; }

    private CompanyLocation()
    {
    }

    internal static CompanyLocation CreatePrivate(
        Guid companyId,
        Guid candidateId,
        string? label,
        string city,
        string country,
        string? address)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            throw new BusinessRuleException(DomainErrors.LocationCityRequired);
        }

        if (string.IsNullOrWhiteSpace(country))
        {
            throw new BusinessRuleException(DomainErrors.LocationCountryRequired);
        }

        return new CompanyLocation
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Label = Normalize(label),
            City = city.Trim(),
            Country = country.Trim(),
            Address = address,
            Visibility = CompanyLocationVisibility.Private,
            CreatedByCandidateId = candidateId
        };
    }

    internal static CompanyLocation CreateShared(
        Guid companyId,
        string? label,
        string city,
        string country,
        string? address)
    {
        CompanyLocation location = CreatePrivate(
            companyId,
            Guid.Empty,
            label,
            city,
            country,
            address);

        location.CreatedByCandidateId = null;
        location.Visibility = CompanyLocationVisibility.Shared;

        return location;
    }

    internal void Approve(Guid reviewerId, DateTime reviewedAt, string? reason)
    {
        Visibility = CompanyLocationVisibility.Shared;
        ReviewedByUserId = reviewerId;
        ReviewedAt = reviewedAt;
        ReviewReason = Normalize(reason);
    }

    internal void Reject(Guid reviewerId, DateTime reviewedAt, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new BusinessRuleException(DomainErrors.ReviewReasonRequired);
        }

        ReviewedByUserId = reviewerId;
        ReviewedAt = reviewedAt;
        ReviewReason = reason.Trim();
    }

    internal void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    internal void UpdateInformation(string? label, string city, string country, string? address)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            throw new BusinessRuleException(DomainErrors.LocationCityRequired);
        }

        if (string.IsNullOrWhiteSpace(country))
        {
            throw new BusinessRuleException(DomainErrors.LocationCountryRequired);
        }

        Label = Normalize(label);
        City = city.Trim();
        Country = country.Trim();
        Address = Normalize(address);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
