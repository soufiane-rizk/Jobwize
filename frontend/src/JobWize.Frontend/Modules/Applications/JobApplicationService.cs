using JobWize.Frontend.Shared.Api;
using JobWize.Frontend.Shared.Authentication;
using JobWize.Frontend.Shared.Results;
using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using CreateJobApplicationContract = JobWize.Modules.Applications.Contracts.Public.JobApplications.CreateJobApplication;
namespace JobWize.Frontend.Modules.Applications;
public sealed class JobApplicationService(IHttpClientFactory httpClientFactory, JobWizeAuthenticationStateProvider authenticationStateProvider) : ApiService(httpClientFactory, authenticationStateProvider)
{
    public Task<Result<GetJobApplications.Response>> GetAsync(CancellationToken cancellationToken = default) => GetAsync<GetJobApplications.Request, GetJobApplications.Response>(GetJobApplications.Route, new(), cancellationToken);
    public Task<Result<CreateJobApplicationContract.Response>> CreateAsync(
        CreateJobApplicationContract.Request request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<CreateJobApplicationContract.Request, CreateJobApplicationContract.Response>(
            CreateJobApplicationContract.Route,
            request,
            cancellationToken);
    }
}
