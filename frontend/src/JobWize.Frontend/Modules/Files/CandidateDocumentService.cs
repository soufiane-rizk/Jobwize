using JobWize.Frontend.Shared.Api;
using JobWize.Frontend.Shared.Authentication;
using JobWize.Frontend.Shared.Results;
using JobWize.Modules.Files.Contracts.Public.FileAssets;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace JobWize.Frontend.Modules.Files;

public sealed class CandidateDocumentService(
    IHttpClientFactory httpClientFactory,
    JobWizeAuthenticationStateProvider authenticationStateProvider)
    : ApiService(httpClientFactory, authenticationStateProvider)
{
    public sealed record DownloadedFile(string FileName, string ContentType, byte[] Content);

    public Task<Result<GetFileAssets.Response>> GetAsync(CancellationToken cancellationToken = default) =>
        GetAsync<object, GetFileAssets.Response>(GetFileAssets.Route, new(), cancellationToken);

    public async Task<Result<UploadFileAsset.Response>> UploadAsync(
        IBrowserFile file,
        CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();
        await using Stream content = file.OpenReadStream(10 * 1024 * 1024, cancellationToken);
        using var fileContent = new StreamContent(content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        form.Add(fileContent, "file", file.Name);

        HttpResponseMessage response = await HttpClient.PostAsync(UploadFileAsset.Route, form, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            UploadFileAsset.Response? result = await response.Content.ReadFromJsonAsync<UploadFileAsset.Response>(cancellationToken);
            return result is null
                ? Result<UploadFileAsset.Response>.Failure(new("Files.EmptyResponse", "The API returned an empty response.", ErrorType.Unexpected))
                : Result<UploadFileAsset.Response>.Success(result);
        }

        return Result<UploadFileAsset.Response>.Failure(new("Files.UploadFailed", await response.Content.ReadAsStringAsync(cancellationToken), ErrorType.Validation));
    }

    public Task<Result> ArchiveAsync(Guid documentId, CancellationToken cancellationToken = default) =>
        DeleteAsync(ArchiveFileAsset.Route, new ArchiveFileAsset.Request(documentId), cancellationToken);

    public async Task<Result<DownloadedFile>> DownloadAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        string route = DownloadFileAsset.Route.Replace("{DocumentId}", documentId.ToString(), StringComparison.Ordinal);
        HttpResponseMessage response = await HttpClient.GetAsync(route, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Result<DownloadedFile>.Failure(new("Files.DownloadFailed", "The document could not be downloaded.", ErrorType.NotFound));
        }

        string fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName
            ?? "document";
        string contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        byte[] content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        return Result<DownloadedFile>.Success(new(fileName.Trim('"'), contentType, content));
    }
}
