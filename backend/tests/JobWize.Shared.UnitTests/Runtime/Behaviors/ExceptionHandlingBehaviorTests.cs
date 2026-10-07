using FluentAssertions;
using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Errors;
using JobWize.Shared.Runtime.Behaviors;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JobWize.Shared.UnitTests.Runtime.Behaviors
{
    public sealed class ExceptionHandlingBehaviorTests
    {
        [Fact]
        public async Task HandleAsync_Should_Return_Handler_Result_When_Handler_Succeeds()
        {
            // Arrange
            RecordingLogger<ExceptionHandlingBehavior<TestCommand, Guid>> logger = new();

            ExceptionHandlingBehavior<TestCommand, Guid> behavior = new(logger);

            Result<Guid> expected = Result<Guid>.Success(Guid.NewGuid());

            // Act
            Result<Guid> result = await behavior.HandleAsync(
                CreateContext(),
                () => Task.FromResult(expected));

            // Assert
            result.Should().BeSameAs(expected);

            logger.Entries.Should().BeEmpty();
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Unexpected_Error_And_Log_When_Handler_Throws()
        {
            // Arrange
            RecordingLogger<ExceptionHandlingBehavior<TestCommand, Guid>> logger = new();

            ExceptionHandlingBehavior<TestCommand, Guid> behavior = new(logger);

            InvalidOperationException exception = new("Test exception.");

            // Act
            Result<Guid> result = await behavior.HandleAsync(
                CreateContext(),
                () => throw exception);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(SharedErrors.Unexpected);

            logger.Entries.Should().ContainSingle(entry =>
                entry.Level == LogLevel.Error &&
                entry.Exception == exception);
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Business_Rule_Error_Without_Logging_An_Unhandled_Error()
        {
            RecordingLogger<ExceptionHandlingBehavior<TestCommand, Guid>> logger = new();
            ExceptionHandlingBehavior<TestCommand, Guid> behavior = new(logger);
            Error expectedError = new(
                "Test.BusinessRule",
                "The requested action is not allowed.",
                ErrorType.Validation);

            Result<Guid> result = await behavior.HandleAsync(
                CreateContext(),
                () => throw new BusinessRuleException(expectedError));

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(expectedError);
            logger.Entries.Should().ContainSingle(entry => entry.Level == LogLevel.Information);
            logger.Entries.Should().NotContain(entry => entry.Level == LogLevel.Error);
        }

        private static ExecutionContext<TestCommand, Result<Guid>> CreateContext()
        {
            return new ExecutionContext<TestCommand, Result<Guid>>(
                new TestCommand(),
                new ServiceCollection().BuildServiceProvider(),
                CancellationToken.None);
        }

        private sealed record TestCommand : ICommand<Guid>;

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
