using FluentAssertions;
using JobWize.Api.Exceptions;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace JobWize.Api.UnitTests.Exceptions
{
    public sealed class GlobalExceptionHandlerTests
    {
        [Fact]
        public async Task TryHandleAsync_Should_Log_And_Write_Unexpected_Error_Response()
        {
            // Arrange
            RecordingLogger<GlobalExceptionHandler> logger = new();
            GlobalExceptionHandler handler = new(logger);

            DefaultHttpContext context = CreateContext();
            InvalidOperationException exception = new("Test exception.");

            // Act
            bool handled = await handler.TryHandleAsync(
                context,
                exception,
                CancellationToken.None);

            context.Response.Body.Position = 0;

            JsonDocument response = await JsonDocument.ParseAsync(context.Response.Body);

            // Assert
            handled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            response.RootElement.GetProperty("code").GetString()
                .Should().Be(SharedErrors.Unexpected.Code);

            logger.Entries.Should().ContainSingle(entry =>
                entry.Level == LogLevel.Error &&
                entry.Exception == exception);
        }

        private static DefaultHttpContext CreateContext()
        {
            DefaultHttpContext context = new();
            context.Response.Body = new MemoryStream();
            context.RequestServices = new ServiceCollection()
                .AddOptions()
                .AddLogging()
                .BuildServiceProvider();

            return context;
        }

        private sealed class RecordingLogger<T> : ILogger<T>
        {
            public List<LogEntry> Entries { get; } = [];

            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                Entries.Add(new LogEntry(logLevel, exception));
            }
        }

        private sealed record LogEntry(LogLevel Level, Exception? Exception);
    }
}
