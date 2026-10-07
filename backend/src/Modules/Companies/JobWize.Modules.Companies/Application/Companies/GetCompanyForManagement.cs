using JobWize.Modules.Companies.Contracts.Public.Companies;
using JobWize.Modules.Companies.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Companies.Application.Companies;

public static class GetCompanyForManagement
{
    internal sealed record Query(Guid Id)
        : IQuery<Contracts.Public.Companies.GetCompanyForManagement.Response>;

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    Contracts.Public.Companies.GetCompanyForManagement.Route,
                    async (
                        Guid id,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<Contracts.Public.Companies.GetCompanyForManagement.Response> result =
                            await dispatcher.SendAsync(new Query(id), cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization(global::JobWize.Modules.Identity.Contracts.Public.Authentication.AuthenticationPolicies.UserManagement)
                .WithName("GetCompanyForManagement")
                .WithTags("Companies");
        }
    }

    internal sealed class Handler(CompaniesDbContext dbContext)
        : IQueryHandler<Query, Contracts.Public.Companies.GetCompanyForManagement.Response>
    {
        public async Task<Result<Contracts.Public.Companies.GetCompanyForManagement.Response>> HandleAsync(
            Query query,
            CancellationToken cancellationToken)
        {
            Contracts.Public.Companies.GetCompanyForManagement.Response? company =
                await dbContext.Companies
                    .AsNoTracking()
                    .Where(item => item.Id == query.Id)
                    .Select(item => new Contracts.Public.Companies.GetCompanyForManagement.Response(
                        item.Id,
                        item.Name,
                        item.Website,
                        item.Industry,
                        item.Description,
                        item.Visibility,
                        item.Locations
                            .Where(location =>
                                location.Visibility == CompanyLocationVisibility.Shared)
                            .OrderBy(location => location.City)
                            .ThenBy(location => location.Country)
                            .Select(location => new Contracts.Public.Companies.GetCompanyForManagement.Location(
                                location.Id,
                                location.Label,
                                location.City,
                                location.Country,
                                location.Address,
                                location.Visibility,
                                location.IsActive,
                                location.CreatedByCandidateId,
                                location.ReviewedAt,
                                location.ReviewReason))
                            .ToList(),
                        item.Contacts
                            .Where(contact =>
                                contact.Visibility == Contracts.Public.CompanyContacts.CompanyContactVisibility.Shared)
                            .OrderBy(contact => contact.Name)
                            .Select(contact => new Contracts.Public.Companies.GetCompanyForManagement.Contact(
                                contact.Id,
                                contact.CompanyLocationId,
                                contact.Name,
                                contact.RoleTitle,
                                contact.Email,
                                contact.PhoneNumber,
                                contact.Visibility,
                                contact.IsActive,
                                contact.CreatedByCandidateId,
                                contact.ReviewedAt,
                                contact.ReviewReason))
                            .ToList()))
                    .SingleOrDefaultAsync(cancellationToken);

            return company is null
                ? Result<Contracts.Public.Companies.GetCompanyForManagement.Response>.Failure(CompaniesErrors.CompanyNotFound)
                : Result<Contracts.Public.Companies.GetCompanyForManagement.Response>.Success(company);
        }
    }
}
