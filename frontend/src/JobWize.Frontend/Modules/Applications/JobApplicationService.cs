using JobWize.Frontend.Shared.Api;
using JobWize.Frontend.Shared.Authentication;
using JobWize.Frontend.Shared.Results;
using JobWize.Modules.Applications.Contracts.Public.JobApplications;
using CreateJobApplicationContract = JobWize.Modules.Applications.Contracts.Public.JobApplications.CreateJobApplication;
using ChangeStatusContract = JobWize.Modules.Applications.Contracts.Public.JobApplications.ChangeJobApplicationStatus;
using AddNoteContract = JobWize.Modules.Applications.Contracts.Public.JobApplications.AddJobApplicationNote;
using ScheduleInterviewContract = JobWize.Modules.Applications.Contracts.Public.Interviews.ScheduleInterview;
using UpdateInterviewContract = JobWize.Modules.Applications.Contracts.Public.Interviews.UpdateInterview;
using RecordInterviewResultContract = JobWize.Modules.Applications.Contracts.Public.Interviews.RecordInterviewResult;
using GetSelectableCompaniesContract = JobWize.Modules.Applications.Contracts.Public.Companies.GetSelectableCompanies;
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

    public Task<Result<GetSelectableCompaniesContract.Response>> GetSelectableCompaniesAsync(
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<GetSelectableCompaniesContract.Request, GetSelectableCompaniesContract.Response>(
            GetSelectableCompaniesContract.Route,
            new(search),
            cancellationToken);
    }

    public Task<Result<GetJobApplication.Response>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<GetJobApplication.Request, GetJobApplication.Response>(
            GetJobApplication.Route,
            new GetJobApplication.Request(id),
            cancellationToken);
    }

    public Task<Result<bool>> ChangeStatusAsync(
        ChangeStatusContract.Request request,
        CancellationToken cancellationToken = default)
    {
        return PatchAsync<ChangeStatusContract.Request, bool>(
            ChangeStatusContract.Route,
            request,
            cancellationToken);
    }

    public Task<Result<bool>> AddNoteAsync(
        AddNoteContract.Request request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<AddNoteContract.Request, bool>(
            AddNoteContract.Route,
            request,
            cancellationToken);
    }

    public Task<Result<ScheduleInterviewContract.Response>> ScheduleInterviewAsync(
        ScheduleInterviewContract.Request request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<ScheduleInterviewContract.Request, ScheduleInterviewContract.Response>(
            ScheduleInterviewContract.Route,
            request,
            cancellationToken);
    }

    public Task<Result<bool>> UpdateInterviewAsync(
        UpdateInterviewContract.Request request,
        CancellationToken cancellationToken = default)
    {
        return PutAsync<UpdateInterviewContract.Request, bool>(
            UpdateInterviewContract.Route,
            request,
            cancellationToken);
    }

    public Task<Result<bool>> RecordInterviewResultAsync(
        RecordInterviewResultContract.Request request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<RecordInterviewResultContract.Request, bool>(
            RecordInterviewResultContract.Route,
            request,
            cancellationToken);
    }
}
