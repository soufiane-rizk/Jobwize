using JobWize.Modules.Companies.Contracts.Public.Companies;
using JobWize.Modules.Companies.Contracts.Public.CompanyContacts;
using JobWize.Shared.Domain;
using JobWize.Shared.Errors;

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
    private readonly List<CompanyContact> _contacts = [];
    public IReadOnlyCollection<CompanyContact> Contacts => _contacts.AsReadOnly();

    private Company()
    {
    }

    public static Company CreatePrivate(
        Guid candidateId,
        string name,
        string? website,
        string? industry,
        string? description,
        IEnumerable<(string? Label, string City, string Country, string? Address)> locations,
        IEnumerable<(int? LocationIndex, string Name, string? RoleTitle, string? Email, string? PhoneNumber)>? contacts = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleException(DomainErrors.CompanyNameRequired);
        }

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

        foreach ((string? label, string city, string country, string? address) in locations)
        {
            company.AddPrivateLocation(candidateId, label, city, country, address);
        }

        foreach ((int? locationIndex, string contactName, string? roleTitle, string? email, string? phoneNumber) in contacts ?? [])
        {
            Guid? locationId = locationIndex is null ? null : company._locations.ElementAt(locationIndex.Value).Id;
            company.AddPrivateContact(candidateId, locationId, contactName, roleTitle, email, phoneNumber);
        }

        return company;
    }

    public static Company CreateShared(
        string name,
        string? website,
        string? industry,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleException(DomainErrors.CompanyNameRequired);
        }

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

    public void AddPrivateLocation(Guid candidateId, string? label, string city, string country, string? address)
    {
        _locations.Add(CompanyLocation.CreatePrivate(Id, candidateId, label, city, country, Normalize(address)));
    }

    public CompanyLocation AddSharedLocation(string? label, string city, string country, string? address)
    {
        CompanyLocation location = CompanyLocation.CreateShared(
            Id,
            label,
            city,
            country,
            Normalize(address));

        _locations.Add(location);

        return location;
    }

    public void UpdateLocation(Guid locationId, string? label, string city, string country, string? address)
    {
        GetLocation(locationId).UpdateInformation(label, city, country, address);
    }

    public CompanyContact AddPrivateContact(
        Guid candidateId,
        Guid? companyLocationId,
        string name,
        string? roleTitle,
        string? email,
        string? phoneNumber)
    {
        ValidateLocationForCandidate(companyLocationId, candidateId);

        CompanyContact contact = CompanyContact.CreatePrivate(
            Id,
            companyLocationId,
            candidateId,
            name,
            roleTitle,
            email,
            phoneNumber);

        _contacts.Add(contact);

        return contact;
    }

    public CompanyContact AddSharedContact(
        Guid? companyLocationId,
        string name,
        string? roleTitle,
        string? email,
        string? phoneNumber)
    {
        if (!IsSharedActiveLocation(companyLocationId))
        {
            throw new BusinessRuleException(DomainErrors.SharedContactRequiresActiveSharedLocation);
        }

        CompanyContact contact = CompanyContact.CreateShared(
            Id,
            companyLocationId,
            name,
            roleTitle,
            email,
            phoneNumber);

        _contacts.Add(contact);

        return contact;
    }

    public void ApproveContact(
        Guid contactId,
        Guid reviewerId,
        DateTime reviewedAt,
        string? reason,
        Guid? companyLocationId,
        string name,
        string? roleTitle,
        string? email,
        string? phoneNumber)
    {
        CompanyContact contact = GetContact(contactId);

        ValidateLocation(companyLocationId);

        if (Visibility != CompanyVisibility.Shared)
        {
            throw new BusinessRuleException(DomainErrors.CompanyMustBeSharedBeforeContactApproval);
        }

        if (!IsSharedActiveLocation(companyLocationId))
        {
            throw new BusinessRuleException(DomainErrors.SharedContactRequiresActiveSharedLocation);
        }

        contact.UpdateInformation(companyLocationId, name, roleTitle, email, phoneNumber);
        contact.Approve(reviewerId, reviewedAt, reason);
    }

    public void RejectContact(Guid contactId, Guid reviewerId, DateTime reviewedAt, string reason)
    {
        CompanyContact contact = GetContact(contactId);
        contact.Reject(reviewerId, reviewedAt, reason);
    }

    public void Approve(Guid reviewerId, DateTime reviewedAt, string? reason, bool approvePendingChildren = true)
    {
        if (Visibility == CompanyVisibility.Shared)
        {
            throw new BusinessRuleException(DomainErrors.CompanyCannotBeReviewedAgain);
        }

        Visibility = CompanyVisibility.Shared;
        ReviewedByUserId = reviewerId;
        ReviewedAt = reviewedAt;
        ReviewReason = Normalize(reason);

        if (!approvePendingChildren)
        {
            return;
        }

        foreach (CompanyContact contact in _contacts.Where(contact =>
                     contact.Visibility == CompanyContactVisibility.Private &&
                     contact.ReviewedAt is null))
        {
            contact.Approve(reviewerId, reviewedAt, reason);
        }

        foreach (CompanyLocation location in _locations.Where(location =>
                     location.Visibility == CompanyLocationVisibility.Private &&
                     location.ReviewedAt is null))
        {
            location.Approve(reviewerId, reviewedAt, reason);
        }

    }

    public void ApproveLocation(Guid locationId, Guid reviewerId, DateTime reviewedAt, string? reason)
    {
        GetLocation(locationId).Approve(reviewerId, reviewedAt, reason);
    }

    public void RejectLocation(Guid locationId, Guid reviewerId, DateTime reviewedAt, string reason)
    {
        GetLocation(locationId).Reject(reviewerId, reviewedAt, reason);
    }

    public void SetLocationActive(Guid locationId, bool isActive)
    {
        GetLocation(locationId).SetActive(isActive);
    }

    public void UpdateContact(
        Guid contactId,
        Guid? companyLocationId,
        string name,
        string? roleTitle,
        string? email,
        string? phoneNumber)
    {
        ValidateLocation(companyLocationId);
        GetContact(contactId).UpdateInformation(
            companyLocationId,
            name,
            roleTitle,
            email,
            phoneNumber);
    }

    public void SetContactActive(Guid contactId, bool isActive)
    {
        GetContact(contactId).SetActive(isActive);
    }

    public bool IsSharedActiveLocation(Guid? locationId)
    {
        if (locationId is null)
        {
            return true;
        }

        CompanyLocation? location = _locations.SingleOrDefault(item => item.Id == locationId);

        return location is not null &&
               location.IsActive &&
               location.Visibility == CompanyLocationVisibility.Shared;
    }

    public bool HasInvalidActiveSharedContactLocation()
    {
        return _contacts.Any(contact =>
            contact.IsActive &&
            contact.Visibility == CompanyContactVisibility.Shared &&
            !IsSharedActiveLocation(contact.CompanyLocationId));
    }

    public void EnsureActiveSharedContactsUseActiveLocations()
    {
        if (HasInvalidActiveSharedContactLocation())
        {
            throw new BusinessRuleException(DomainErrors.SharedContactRequiresActiveSharedLocation);
        }
    }

    public void UpdateBasicInformation(
        string? name,
        string? website,
        string? industry,
        string? description)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(website))
        {
            Website = website.Trim();
        }

        if (!string.IsNullOrWhiteSpace(industry))
        {
            Industry = industry.Trim();
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            Description = description.Trim();
        }
    }

    public void ReplaceBasicInformation(
        string name,
        string? website,
        string? industry,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleException(DomainErrors.CompanyNameRequired);
        }

        Name = name.Trim();
        Website = Normalize(website);
        Industry = Normalize(industry);
        Description = Normalize(description);
    }

    public void Reject(Guid reviewerId, DateTime reviewedAt, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new BusinessRuleException(DomainErrors.ReviewReasonRequired);
        }

        if (Visibility == CompanyVisibility.Shared)
        {
            throw new BusinessRuleException(DomainErrors.CompanyCannotBeReviewedAgain);
        }

        Visibility = CompanyVisibility.Private;
        ReviewedByUserId = reviewerId;
        ReviewedAt = reviewedAt;
        ReviewReason = reason.Trim();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private void ValidateLocation(Guid? companyLocationId)
    {
        if (companyLocationId is not null && _locations.All(location => location.Id != companyLocationId))
        {
            throw new BusinessRuleException(DomainErrors.LocationNotInCompany);
        }
    }

    private void ValidateLocationForCandidate(Guid? companyLocationId, Guid candidateId)
    {
        if (companyLocationId is null)
        {
            return;
        }

        CompanyLocation? location = _locations.SingleOrDefault(item => item.Id == companyLocationId);

        if (location is null ||
            !location.IsActive ||
            (location.Visibility != CompanyLocationVisibility.Shared &&
             location.CreatedByCandidateId != candidateId))
        {
            throw new BusinessRuleException(DomainErrors.LocationNotSelectable);
        }
    }

    private CompanyContact GetContact(Guid contactId)
    {
        return _contacts.SingleOrDefault(contact => contact.Id == contactId)
            ?? throw new BusinessRuleException(DomainErrors.CompanyContactNotInCompany);
    }

    private CompanyLocation GetLocation(Guid locationId)
    {
        return _locations.SingleOrDefault(location => location.Id == locationId)
            ?? throw new BusinessRuleException(DomainErrors.CompanyLocationNotInCompany);
    }
}
