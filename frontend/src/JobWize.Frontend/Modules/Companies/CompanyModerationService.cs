using JobWize.Frontend.Shared.Api;
using JobWize.Frontend.Shared.Authentication;
using JobWize.Frontend.Shared.Results;
using JobWize.Modules.Companies.Contracts.Public.Companies;

namespace JobWize.Frontend.Modules.Companies;

public sealed class CompanyModerationService(IHttpClientFactory httpClientFactory, JobWizeAuthenticationStateProvider authenticationStateProvider) : ApiService(httpClientFactory, authenticationStateProvider)
{
    public Task<Result<GetCompaniesForReview.Response>> GetForReviewAsync(CancellationToken cancellationToken = default) => GetAsync<object, GetCompaniesForReview.Response>(GetCompaniesForReview.Route, new(), cancellationToken);
    public Task<Result<bool>> ReviewAsync(ReviewCompany.Request request, CancellationToken cancellationToken = default) =>
        PostAsync<ReviewCompany.Request, bool>(ReviewCompany.Route, request, cancellationToken);
}
