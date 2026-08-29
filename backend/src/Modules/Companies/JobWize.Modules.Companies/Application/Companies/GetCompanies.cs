using JobWize.Modules.Companies.Contracts.Public.Companies;
using JobWize.Modules.Companies.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Companies.Application.Companies;

public static class GetCompanies
{
    internal sealed record Query(string? Search) : IQuery<Contracts.Public.Companies.GetCompanies.Response>;

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    Contracts.Public.Companies.GetCompanies.Route,
                    async (
                        string? search,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<Contracts.Public.Companies.GetCompanies.Response> result =
                            await dispatcher.SendAsync(new Query(search), cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("GetCompanies")
                .WithTags("Companies");
        }
    }

    internal sealed class Handler(
        CompaniesDbContext dbContext,
        IUserContext userContext) : IQueryHandler<Query, Contracts.Public.Companies.GetCompanies.Response>
    {
        public async Task<Result<Contracts.Public.Companies.GetCompanies.Response>> HandleAsync(Query query, CancellationToken cancellationToken)
        {
            IQueryable<Domain.Company> companies = dbContext.Companies
                .AsNoTracking()
                .Include(company => company.Locations)
                .Where(company =>
                    company.Visibility == CompanyVisibility.Shared ||
                    company.CreatedByCandidateId == userContext.UserId);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string search = query.Search.Trim();
                companies = companies.Where(company => EF.Functions.ILike(company.Name, $"%{search}%"));
            }

            var items = await companies
                .OrderBy(company => company.Name)
                .Select(company => new Contracts.Public.Companies.GetCompanies.Item(
                    company.Id,
                    company.Name,
                    company.Website,
                    company.Industry,
                    company.Description,
                    company.Visibility,
                    company.Locations
                        .Where(location =>
                            location.IsActive &&
                            (location.Visibility == CompanyLocationVisibility.Shared ||
                             location.CreatedByCandidateId == userContext.UserId))
                        .OrderBy(location => location.City)
                        .ThenBy(location => location.Country)
                        .ThenBy(location => location.Label)
                        .Select(location => new Contracts.Public.Companies.GetCompanies.Location(
                            location.Id,
                            location.Label ?? (location.City + ", " + location.Country),
                            location.City,
                            location.Country,
                            location.Address))
                        .ToList()))
                .ToListAsync(cancellationToken);

            return Result<Contracts.Public.Companies.GetCompanies.Response>.Success(new(items));
        }
    }
}
