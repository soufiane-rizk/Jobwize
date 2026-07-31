using FluentAssertions;
using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Runtime.UnitTests.Helpers.Transactions;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Runtime.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Transactions
{
    public sealed class TransactionBehaviorTests
    {
        private static ExecutionContext<FakeCommand, Result<Guid>> CreateContext()
        {
            return new ExecutionContext<FakeCommand, Result<Guid>>(
                new FakeCommand(),
                new ServiceCollection().BuildServiceProvider(),
                CancellationToken.None);
        }

        [Fact]
        public async Task HandleAsync_Should_Begin_Persist_And_Commit_When_Command_Succeeds()
        {
            // Arrange
            RecordingTransactionManager transactionManager = new();

            TransactionBehavior<FakeCommand, Guid> behavior =
                new(transactionManager);

            // Act
            Result<Guid> result = await behavior.HandleAsync(
                CreateContext(),
                () => Task.FromResult(Result<Guid>.Success(Guid.NewGuid())));

            // Assert
            result.IsSuccess.Should().BeTrue();

            transactionManager.Calls.Should().Equal(
                "Begin",
                "Persist",
                "Commit");
        }

        [Fact]
        public async Task HandleAsync_Should_Begin_And_Rollback_When_Command_Returns_Failure()
        {
            // Arrange
            RecordingTransactionManager transactionManager = new();

            TransactionBehavior<FakeCommand, Guid> behavior =
                new(transactionManager);

            // Act
            Result<Guid> result = await behavior.HandleAsync(
                CreateContext(),
                () => Task.FromResult(
                    Result<Guid>.Failure(
                        new Error("Test", "Failure", ErrorType.Failure))));

            // Assert
            result.IsFailure.Should().BeTrue();

            transactionManager.Calls.Should().Equal(
                "Begin",
                "Rollback");
        }

        [Fact]
        public async Task HandleAsync_Should_Begin_And_Rollback_When_Handler_Throws()
        {
            // Arrange
            RecordingTransactionManager transactionManager = new();

            TransactionBehavior<FakeCommand, Guid> behavior =
                new(transactionManager);

            // Act
            Func<Task> act = () =>
                behavior.HandleAsync(
                    CreateContext(),
                    () => throw new InvalidOperationException());

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>();

            transactionManager.Calls.Should().Equal(
                "Begin",
                "Rollback");
        }

        [Fact]
        public async Task HandleAsync_Should_Not_Commit_When_Command_Returns_Failure()
        {
            // Arrange
            RecordingTransactionManager transactionManager = new();

            TransactionBehavior<FakeCommand, Guid> behavior =
                new(transactionManager);

            // Act
            await behavior.HandleAsync(
                CreateContext(),
                () => Task.FromResult(
                    Result<Guid>.Failure(
                        new Error("Test", "Failure", ErrorType.Failure))));

            // Assert
            transactionManager.Calls.Should().NotContain("Commit");
        }

        [Fact]
        public async Task HandleAsync_Should_Not_Persist_When_Command_Returns_Failure()
        {
            // Arrange
            RecordingTransactionManager transactionManager = new();

            TransactionBehavior<FakeCommand, Guid> behavior =
                new(transactionManager);

            // Act
            await behavior.HandleAsync(
                CreateContext(),
                () => Task.FromResult(
                    Result<Guid>.Failure(
                        new Error("Test", "Failure", ErrorType.Failure))));

            // Assert
            transactionManager.Calls.Should().NotContain("Persist");
        }
    }
}
