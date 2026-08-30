using JobWize.Modules.Companies.Persistence;
using JobWize.Runtime.Contracts.Requests;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Companies.Application.Companies;

internal sealed class GetAllCompanyProjectionsHandler(CompaniesDbContext dbContext)
    : IModuleQueryHandler<Contracts.Internal.Companies.GetAllCompanyProjections.Query, IReadOnlyList<Contracts.Internal.Companies.GetCompanyProjection.Response>>
{
    public async Task<IReadOnlyList<Contracts.Internal.Companies.GetCompanyProjection.Response>> HandleAsync(
        Contracts.Internal.Companies.GetAllCompanyProjections.Query query,
        CancellationToken cancellationToken)
    {
        return await dbContext.Companies
            .AsNoTracking()
            .Include(company => company.Locations)
            .Include(company => company.Contacts)
            .Select(company => new Contracts.Internal.Companies.GetCompanyProjection.Response(
                company.Id,
                company.Name,
                company.Visibility,
                company.CreatedByCandidateId,
                company.Locations
                    .OrderBy(location => location.City)
                    .ThenBy(location => location.Country)
                    .ThenBy(location => location.Label)
                    .Select(location => new Contracts.Internal.Companies.GetCompanyProjection.Location(
                        location.Id,
                        location.CompanyId,
                        location.Label ?? (location.City + ", " + location.Country),
                        location.Visibility,
                        location.CreatedByCandidateId,
                        location.IsActive))
                    .ToList(),
                company.Contacts
                    .OrderBy(contact => contact.Name)
                    .Select(contact => new Contracts.Internal.Companies.GetCompanyProjection.Contact(
                        contact.Id,
                        contact.CompanyId,
                        contact.CompanyLocationId,
                        contact.Name,
                        contact.RoleTitle,
                        contact.Email,
                        contact.PhoneNumber,
                        contact.Visibility,
                        contact.CreatedByCandidateId,
                        contact.IsActive,
                        contact.Visibility == Contracts.Public.CompanyContacts.CompanyContactVisibility.Private &&
                        contact.ReviewedAt != null))
                    .ToList()))
            .ToListAsync(cancellationToken);
    }
}
