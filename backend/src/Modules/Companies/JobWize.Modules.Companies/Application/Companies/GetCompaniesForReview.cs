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

public static class GetCompaniesForReview
{
    internal sealed record Query : IQuery<Contracts.Public.Companies.GetCompaniesForReview.Response>;

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(Contracts.Public.Companies.GetCompaniesForReview.Route, async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
            {
                Result<Contracts.Public.Companies.GetCompaniesForReview.Response> result = await dispatcher.SendAsync(new Query(), cancellationToken);
                return result.ToApiResult();
            })
            .RequireAuthorization(global::JobWize.Modules.Identity.Contracts.Public.Authentication.AuthenticationPolicies.UserManagement)
            .WithName("GetCompaniesForReview")
            .WithTags("Companies");
        }
    }

    internal sealed class Handler(CompaniesDbContext dbContext) : IQueryHandler<Query, Contracts.Public.Companies.GetCompaniesForReview.Response>
    {
        public async Task<Result<Contracts.Public.Companies.GetCompaniesForReview.Response>> HandleAsync(Query query, CancellationToken cancellationToken)
        {
            var companies = await dbContext.Companies.AsNoTracking().Include(company => company.Locations)
                .Where(company => company.Visibility == CompanyVisibility.Private && company.CreatedByCandidateId != null && company.ReviewedAt == null)
                .OrderBy(company => company.CreatedAt)
                .Select(company => new Contracts.Public.Companies.GetCompaniesForReview.Item(company.Id, company.Name, company.Website, company.Industry, company.Description, company.CreatedByCandidateId!.Value, company.CreatedAt, company.Locations.OrderBy(location => location.Label).Select(location => new Contracts.Public.Companies.GetCompanies.Location(location.Id, location.Label, location.City, location.Country, location.Address)).ToList()))
                .ToListAsync(cancellationToken);
            return Result<Contracts.Public.Companies.GetCompaniesForReview.Response>.Success(new(companies));
        }
    }
}
