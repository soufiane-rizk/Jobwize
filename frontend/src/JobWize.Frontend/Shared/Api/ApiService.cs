using JobWize.Frontend.Shared.Authentication;
using JobWize.Frontend.Shared.Results;
using JobWize.Shared.Contracts.Http.Attributes;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Error = JobWize.Frontend.Shared.Results.Error;

namespace JobWize.Frontend.Shared.Api
{
    public abstract class ApiService
    {
        protected readonly HttpClient HttpClient;
        private readonly JobWizeAuthenticationStateProvider _authenticationStateProvider;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        protected ApiService(IHttpClientFactory httpClientFactory, JobWizeAuthenticationStateProvider authenticationStateProvider)
        {
            HttpClient = httpClientFactory.CreateClient("Api");
            _authenticationStateProvider = authenticationStateProvider;
        }

        protected async Task<Result<TResponse>> GetAsync<TRequest, TResponse>(string route, TRequest request, CancellationToken cancellationToken = default)
        {
            RequestDescriptor descriptor = BuildRequest(route, request);

            try
            {

                HttpResponseMessage response = await HttpClient.GetAsync(descriptor.Url, cancellationToken);

                Result<TResponse> result = await ReadResponseAsync<TResponse>(response);

                await OnResultReceivedAsync(result);

                return result;
            }

            catch (Exception exception)
            {
                throw new ApiRequestException(
                    HttpMethod.Post.Method,
                    descriptor.Url,
                    "An unexpected error occurred while executing the request.",
                    exception);
            }
        }

        protected async Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string route, TRequest request, CancellationToken cancellationToken = default)
        {
            RequestDescriptor descriptor = BuildRequest(route, request);

            try
            {
                HttpResponseMessage response = await HttpClient.PostAsJsonAsync(descriptor.Url, descriptor.Body, cancellationToken);

                var result = await ReadResponseAsync<TResponse>(response);

                await OnResultReceivedAsync(result);

                return result;
            }

            catch (Exception exception)
            {
                throw new ApiRequestException(
                    HttpMethod.Post.Method,
                    descriptor.Url,
                    "An unexpected error occurred while executing the request.",
                    exception);
            }
        }

        protected async Task<Result> PostAsync<TRequest>(string route, TRequest request, CancellationToken cancellationToken = default)
        {
            RequestDescriptor descriptor = BuildRequest(route, request);

            try
            {
                HttpResponseMessage response = await HttpClient.PostAsJsonAsync(descriptor.Url, descriptor.Body, cancellationToken);

                Result result = await ReadResponseAsync(response);

                await OnResultReceivedAsync(result);

                return result;
            }

            catch (Exception exception)
            {
                throw new ApiRequestException(
                    HttpMethod.Post.Method,
                    descriptor.Url,
                    "An unexpected error occurred while executing the request.",
                    exception);
            }
        }

        protected async Task<Result<TResponse>> PutAsync<TRequest, TResponse>(string route, TRequest request, CancellationToken cancellationToken = default)
        {
            RequestDescriptor descriptor = BuildRequest(route, request);

            try
            {
                HttpResponseMessage response = await HttpClient.PutAsJsonAsync(descriptor.Url, descriptor.Body, cancellationToken);

                Result<TResponse> result = await ReadResponseAsync<TResponse>(response);

                await OnResultReceivedAsync(result);

                return result;
            }

            catch (Exception exception)
            {
                throw new ApiRequestException(
                    HttpMethod.Put.Method,
                    descriptor.Url,
                    "An unexpected error occurred while executing the request.",
                    exception);
            }
        }

        protected async Task<Result> PutAsync<TRequest>(string route, TRequest request, CancellationToken cancellationToken = default)
        {
            RequestDescriptor descriptor = BuildRequest(route, request);

            try
            {
                HttpResponseMessage response = await HttpClient.PutAsJsonAsync(descriptor.Url, descriptor.Body, cancellationToken);

                Result result = await ReadResponseAsync(response);

                await OnResultReceivedAsync(result);

                return result;
            }

            catch (Exception exception)
            {
                throw new ApiRequestException(
                    HttpMethod.Put.Method,
                    descriptor.Url,
                    "An unexpected error occurred while executing the request.",
                    exception);
            }
        }

        protected async Task<Result<TResponse>> PatchAsync<TRequest, TResponse>(string route, TRequest request, CancellationToken cancellationToken = default)
        {
            RequestDescriptor descriptor = BuildRequest(route, request);

            try
            {
                HttpResponseMessage response = await HttpClient.PatchAsJsonAsync(descriptor.Url, descriptor.Body, cancellationToken);

                Result<TResponse> result = await ReadResponseAsync<TResponse>(response);

                await OnResultReceivedAsync(result);

                return result;
            }

            catch (Exception exception)
            {
                throw new ApiRequestException(
                    HttpMethod.Patch.Method,
                    descriptor.Url,
                    "An unexpected error occurred while executing the request.",
                    exception);
            }
        }

        protected async Task<Result> PatchAsync<TRequest>(string route, TRequest request, CancellationToken cancellationToken = default)
        {
            RequestDescriptor descriptor = BuildRequest(route, request);

            try
            {
                HttpResponseMessage response = await HttpClient.PatchAsJsonAsync(descriptor.Url, descriptor.Body, cancellationToken);

                Result result = await ReadResponseAsync(response);

                await OnResultReceivedAsync(result);

                return result;
            }

            catch (Exception exception) {
                throw new ApiRequestException(
                    HttpMethod.Patch.Method,
                    descriptor.Url,
                    "An unexpected error occurred while executing the request.",
                    exception);
            }
        }

        protected async Task<Result> DeleteAsync<TRequest>(string route, TRequest request, CancellationToken cancellationToken = default)
        {
            RequestDescriptor descriptor = BuildRequest(route, request);

            try
            {
                HttpResponseMessage response = await HttpClient.DeleteAsync(descriptor.Url, cancellationToken);

                Result result = await ReadResponseAsync(response);

                await OnResultReceivedAsync(result);

                return result;
            }
            catch (Exception exception)
            {
                throw new ApiRequestException(
                    HttpMethod.Delete.Method,
                    descriptor.Url,
                    "An unexpected error occurred while executing the request.",
                    exception);
            }
        }

        private static RequestDescriptor BuildRequest<TRequest>(string route, TRequest request)
        {
            string url = route;

            Dictionary<string, object?> body = [];

            List<KeyValuePair<string, string?>> queryParameters = [];

            PropertyInfo[] properties = typeof(TRequest).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                object? value = property.GetValue(request);

                HttpRouteAttribute? routeAttribute = property.GetCustomAttribute<HttpRouteAttribute>();

                HttpQueryAttribute? queryAttribute = property.GetCustomAttribute<HttpQueryAttribute>();

                HttpBodyAttribute? bodyAttribute = property.GetCustomAttribute<HttpBodyAttribute>();

                int transportAttributeCount =
                    (routeAttribute is not null ? 1 : 0) +
                    (queryAttribute is not null ? 1 : 0) +
                    (bodyAttribute is not null ? 1 : 0);

                if (transportAttributeCount == 0)
                {
                    throw new InvalidOperationException(
                        $"Property '{property.Name}' must have exactly one HTTP transport attribute.");
                }

                if (transportAttributeCount > 1)
                {
                    throw new InvalidOperationException(
                        $"Property '{property.Name}' has multiple HTTP transport attributes.");
                }

                if (routeAttribute is not null)
                {
                    string placeholder = $"{{{property.Name}}}";

                    if (!url.Contains(placeholder, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            $"Property '{property.Name}' is marked with [HttpRoute] but the route template does not contain '{placeholder}'.");
                    }

                    url = url.Replace(
                        $"{{{property.Name}}}",
                        Uri.EscapeDataString(value?.ToString() ?? string.Empty));

                    continue;
                }

                if (queryAttribute is not null)
                {
                    queryParameters.Add(new(
                        property.Name,
                        value?.ToString()));

                    continue;
                }

                if (bodyAttribute is not null)
                {
                    body.Add(
                        property.Name,
                        value);

                    continue;
                }
            }

            if (queryParameters.Count > 0)
            {
                string query = string.Join(
                    "&",
                    queryParameters.Select(x =>
                        $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value ?? string.Empty)}"));

                url = $"{url}?{query}";
            }

            if (Regex.IsMatch(url, @"\{.+?\}"))
            {
                throw new InvalidOperationException(
                    $"Route template '{route}' contains unresolved route parameters.");
            }

            return new RequestDescriptor
            {
                Url = url,
                Body = body.Count == 0
                    ? null
                    : body
            };
        }

        private static async Task<Result<TResponse>> ReadResponseAsync<TResponse>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                TResponse? value = await response.Content.ReadFromJsonAsync<TResponse>();

                if (value is null)
                {
                    throw new InvalidOperationException(
                        $"The API returned an empty response for '{typeof(TResponse).Name}'.");
                }

                return Result<TResponse>.Success(value);
            }

            ApiProblemDetails? problem = await ReadProblemDetailsAsync(response);

            Error error = CreateError(response.StatusCode, problem);

            return Result<TResponse>.Failure(error);
        }

        private static async Task<Result> ReadResponseAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            ApiProblemDetails? problem = await ReadProblemDetailsAsync(response);

            Error error = CreateError(response.StatusCode, problem);

            return Result.Failure(error);
        }

        private static async Task<ApiProblemDetails?> ReadProblemDetailsAsync(HttpResponseMessage response)
        {
            string content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<ApiProblemDetails>(content, JsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static Error CreateError(HttpStatusCode statusCode, ApiProblemDetails? problem)
        {
            ErrorType type = statusCode switch
            {
                HttpStatusCode.BadRequest => ErrorType.Validation,
                HttpStatusCode.Conflict => ErrorType.Conflict,
                HttpStatusCode.NotFound => ErrorType.NotFound,
                HttpStatusCode.Unauthorized => ErrorType.Unauthorized,
                HttpStatusCode.Forbidden => ErrorType.Forbidden,
                _ => ErrorType.Unexpected
            };

            string code = statusCode.ToString();

            if (problem?.Extensions?.TryGetValue("code", out JsonElement codeElement) == true)
            {
                code = codeElement.GetString() ?? code;
            }

            Dictionary<string, string[]>? validationErrors = null;

            if (problem?.Extensions?.TryGetValue("errors", out JsonElement errorsElement) == true)
            {
                validationErrors = errorsElement.Deserialize<Dictionary<string, string[]>>();
            }

            return new Error(
                code,
                problem?.Detail ?? problem?.Title ?? "An unknown error occurred.",
                type,
                validationErrors);
        }

        protected virtual async Task OnResultReceivedAsync(Result result)
        {
            if (!result.IsFailure)
                return;

            if (result.Error?.Type == ErrorType.Unauthorized &&
                await _authenticationStateProvider.HasTokensAsync())
            {
                await _authenticationStateProvider.LogoutAsync();
            }
        }

        protected virtual Task OnResultReceivedAsync<TResponse>(Result<TResponse> result)
        {
            return OnResultReceivedAsync((Result)result);
        }
    }
}
