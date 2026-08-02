using FluentAssertions;
using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Runtime.UnitTests.Helpers;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Runtime.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Behaviors
{
    public sealed class ValidationBehaviorTests
    {
        private static ExecutionContext<ValidationCommand.Command, Result<Guid>> CreateContext(
            ValidationCommand.Command command)
        {
            return new ExecutionContext<ValidationCommand.Command, Result<Guid>>(
                command,
                new ServiceCollection().BuildServiceProvider(),
                CancellationToken.None);
        }

        [Fact]
        public async Task HandleAsync_Should_Invoke_Handler_When_Validation_Succeeds()
        {
            // Arrange
            ValidationBehavior<ValidationCommand.Command, Guid> behavior =
                new([
                    new ValidationCommand.Validator(),
                    new ValidationCommand.SecondValidator()
                ]);

            bool handlerCalled = false;

            // Act
            Result<Guid> result = await behavior.HandleAsync(
                CreateContext(new ValidationCommand.Command("John", 25)),
                () =>
                {
                    handlerCalled = true;

                    return Task.FromResult(
                        Result<Guid>.Success(Guid.NewGuid()));
                });

            // Assert
            result.IsSuccess.Should().BeTrue();

            handlerCalled.Should().BeTrue();
        }

        [Fact]
        public async Task HandleAsync_Should_Return_Validation_Error_When_Request_Is_Invalid()
        {
            // Arrange
            ValidationBehavior<ValidationCommand.Command, Guid> behavior =
                new([
                    new ValidationCommand.Validator(),
                    new ValidationCommand.SecondValidator()
                ]);

            // Act
            Result<Guid> result = await behavior.HandleAsync(
                CreateContext(new ValidationCommand.Command("", 10)),
                () => Task.FromResult(
                    Result<Guid>.Success(Guid.NewGuid())));

            // Assert
            result.IsFailure.Should().BeTrue();

            result.Error.Type.Should().Be(ErrorType.Validation);

            result.Error.Code.Should().Be("Runtime.ValidationFailed");
        }

        [Fact]
        public async Task HandleAsync_Should_Not_Invoke_Handler_When_Validation_Fails()
        {
            // Arrange
            ValidationBehavior<ValidationCommand.Command, Guid> behavior =
                new([
                    new ValidationCommand.Validator(),
                    new ValidationCommand.SecondValidator()
                ]);

            bool handlerCalled = false;

            // Act
            await behavior.HandleAsync(
                CreateContext(new ValidationCommand.Command("", 10)),
                () =>
                {
                    handlerCalled = true;

                    return Task.FromResult(
                        Result<Guid>.Success(Guid.NewGuid()));
                });

            // Assert
            handlerCalled.Should().BeFalse();
        }

        [Fact]
        public async Task HandleAsync_Should_Return_All_Validation_Errors()
        {
            // Arrange
            ValidationBehavior<ValidationCommand.Command, Guid> behavior =
                new([
                    new ValidationCommand.Validator(),
                    new ValidationCommand.SecondValidator()
                ]);

            // Act
            Result<Guid> result = await behavior.HandleAsync(
                CreateContext(new ValidationCommand.Command("", 10)),
                () => Task.FromResult(
                    Result<Guid>.Success(Guid.NewGuid())));

            result.IsFailure.Should().BeTrue();

            result.Error.Details.Should().HaveCount(3);

            result.Error.Details.Should().Contain(x =>
                x.Field == "Name");

            result.Error.Details.Should().Contain(x =>
                x.Field == "Age");
        }

        [Fact]
        public async Task HandleAsync_Should_Invoke_Handler_When_No_Validators_Are_Registered()
        {
            // Arrange
            ValidationBehavior<ValidationCommand.Command, Guid> behavior =
                new([]);

            bool handlerCalled = false;

            // Act
            Result<Guid> result = await behavior.HandleAsync(
                CreateContext(new ValidationCommand.Command("", 10)),
                () =>
                {
                    handlerCalled = true;

                    return Task.FromResult(
                        Result<Guid>.Success(Guid.NewGuid()));
                });

            // Assert
            result.IsSuccess.Should().BeTrue();

            handlerCalled.Should().BeTrue();
        }
    }
}
