using JobWize.Shared.Application.Results;
using JobWize.Shared.Endpoints;
using Microsoft.AspNetCore.Diagnostics;

namespace JobWize.Api.Exceptions
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Unhandled HTTP request exception.");

            if (httpContext.Response.HasStarted)
            {
                return false;
            }

            await Result.Failure(SharedErrors.Unexpected)
                .ToApiResult()
                .ExecuteAsync(httpContext);

            return true;
        }
    }
}
