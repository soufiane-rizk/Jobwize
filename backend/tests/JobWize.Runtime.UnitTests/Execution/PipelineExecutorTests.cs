using FluentAssertions;
using JobWize.ModuleOne;
using JobWize.ModuleOne.Contracts;
using JobWize.ModuleOne.Features;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Runtime.Execution;
using JobWize.Runtime.UnitTests.Helpers;
using JobWize.Runtime.UnitTests.Helpers.Pipeline;
using JobWize.Shared.Application.Results;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Execution
{
    public sealed class PipelineExecutorTests
    {
        [Fact]
        public async Task SendAsync_Should_Execute_Request_Pipeline()
        {
            // Arrange
            PipelineExecutionRecorder recorder = new();

            (ModuleRuntime runtime, ServiceProvider provider) =
                RuntimeTestFactory.CreateModuleOneRuntimeWithProvider(
                    services =>
                    {
                        services.AddSingleton(recorder);
                    },
                    typeof(RecordingRequestBehavior<,>));

            recorder.Events.Should().BeEmpty();

            CreateItem.Command command = new("Test Item");

            // Act
            Result<Guid> result = await runtime.SendAsync(provider, command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Value.Should().Be(ItemRepository.CreatedId);

            recorder.Events.Should().Equal(
                "Request.Before",
                "Request.After");
        }


        [Fact]
        public async Task SendAsync_Should_Resolve_And_Execute_Command_Pipeline()
        {
            // Arrange
            PipelineExecutionRecorder recorder = new();

            (ModuleRuntime runtime, ServiceProvider provider) =
                RuntimeTestFactory.CreateModuleOneRuntimeWithProvider(
                    services =>
                    {
                        services.AddSingleton(recorder);
                    },
                    typeof(RecordingCommandBehavior<,>));

            recorder.Events.Should().BeEmpty();

            CreateItem.Command command = new("Test Item");

            // Act
            Result<Guid> result =
                await runtime.SendAsync(provider, command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(ItemRepository.CreatedId);

            recorder.Events.Should().Equal(
                "Command.Before",
                "Command.After");
        }

        [Fact]
        public async Task SendAsync_Should_Execute_Query_Pipeline()
        {
            // Arrange
            PipelineExecutionRecorder recorder = new();

            (ModuleRuntime runtime, ServiceProvider provider) =
                RuntimeTestFactory.CreateModuleOneRuntimeWithProvider(
                    services =>
                    {
                        services.AddSingleton(recorder);
                    },
                    typeof(RecordingQueryBehavior<,>));

            recorder.Events.Should().BeEmpty();

            ModuleOne.Features.GetItem.Query query = new(Guid.NewGuid());

            // Act
            Result<ModuleOne.Contracts.GetItem.Response> result =
                await runtime.SendAsync(provider, query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            recorder.Events.Should().Equal(
                "Query.Before",
                "Query.After");
        }

        [Fact]
        public async Task SendAsync_Should_Execute_Request_And_Command_Pipelines_In_Order()
        {
            // Arrange
            PipelineExecutionRecorder recorder = new();

            (ModuleRuntime runtime, ServiceProvider provider) =
                 RuntimeTestFactory.CreateModuleOneRuntimeWithProvider(
                     services =>
                     {
                         services.AddSingleton(recorder);
                     },
                     typeof(RecordingRequestBehavior<,>),
                     typeof(RecordingCommandBehavior<,>));

            CreateItem.Command command = new("Test Item");

            // Act
            Result<Guid> result =
                await runtime.SendAsync(provider, command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            recorder.Events.Should().Equal(
                "Request.Before",
                "Command.Before",
                "Command.After",
                "Request.After");
        }

        [Fact]
        public async Task SendAsync_Should_Execute_Request_And_Query_Pipelines_In_Order()
        {
            // Arrange
            PipelineExecutionRecorder recorder = new();

            (ModuleRuntime runtime, ServiceProvider provider) =
                RuntimeTestFactory.CreateModuleOneRuntimeWithProvider(
                    services =>
                    {
                        services.AddSingleton(recorder);
                    },
                    typeof(RecordingRequestBehavior<,>),
                    typeof(RecordingQueryBehavior<,>));

            ModuleOne.Features.GetItem.Query query = new(Guid.NewGuid());

            // Act
            Result<ModuleOne.Contracts.GetItem.Response> result =
                await runtime.SendAsync(provider, query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            recorder.Events.Should().Equal(
                "Request.Before",
                "Query.Before",
                "Query.After",
                "Request.After");
        }

        [Fact]
        public async Task SendAsync_Should_Not_Execute_Command_Pipeline_For_Query()
        {
            // Arrange
            PipelineExecutionRecorder recorder = new();

            (ModuleRuntime runtime, ServiceProvider provider) =
                RuntimeTestFactory.CreateModuleOneRuntimeWithProvider(
                    services =>
                    {
                        services.AddSingleton(recorder);
                    },
                    typeof(RecordingRequestBehavior<,>),
                    typeof(RecordingCommandBehavior<,>));

            ModuleOne.Features.GetItem.Query query = new(Guid.NewGuid());

            // Act
            Result<ModuleOne.Contracts.GetItem.Response> result =
                await runtime.SendAsync(provider, query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            recorder.Events.Should().Equal(
                "Request.Before",
                "Request.After");
        }

        [Fact]
        public async Task SendAsync_Should_Not_Execute_Query_Pipeline_For_Command()
        {
            // Arrange
            PipelineExecutionRecorder recorder = new();

            (ModuleRuntime runtime, ServiceProvider provider) =
                RuntimeTestFactory.CreateModuleOneRuntimeWithProvider(
                    services =>
                    {
                        services.AddSingleton(recorder);
                    },
                    typeof(RecordingRequestBehavior<,>),
                    typeof(RecordingQueryBehavior<,>));

            CreateItem.Command command = new("Test Item");

            // Act
            Result<Guid> result =
                await runtime.SendAsync(provider, command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            recorder.Events.Should().Equal(
                "Request.Before",
                "Request.After");
        }

        [Fact]
        public async Task SendAsync_Should_Stop_When_Pipeline_Does_Not_Call_Next()
        {
            // Arrange
            PipelineExecutionRecorder recorder = new();

            (ModuleRuntime runtime, ServiceProvider provider) =
                RuntimeTestFactory.CreateModuleOneRuntimeWithProvider(
                    services =>
                    {
                        services.AddSingleton(recorder);
                    },
                    typeof(BlockingCommandBehavior<,>));

            CreateItem.Command command = new("Test Item");

            // Act
            Result<Guid> result =
                await runtime.SendAsync(provider, command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();

            recorder.Events.Should().Equal(
                "Command.Before");
        }

        [Fact]
        public async Task SendAsync_Should_Return_Failure_When_Exception_Is_Handled_By_Pipeline()
        {
            // Arrange
            (ModuleRuntime runtime, ServiceProvider provider) =
                RuntimeTestFactory.CreateModuleOneRuntimeWithProvider(
                    services => { },
                    typeof(ExceptionHandlingCommandBehavior<,>));

            ThrowException.Command command = new();

            // Act
            Result<Guid> result =
                await runtime.SendAsync(provider, command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();

            result.Error.Code.Should().Be("Test.Exception");
            result.Error.Message.Should().Be("Boom!");
        }

        [Fact]
        public async Task SendAsync_Should_Execute_Request_Behaviors_In_Registration_Order()
        {
            // Arrange
            PipelineExecutionRecorder recorder = new();

            (ModuleRuntime runtime, ServiceProvider provider) =
                RuntimeTestFactory.CreateModuleOneRuntimeWithProvider(
                    services =>
                    {
                        services.AddSingleton(recorder);
                    },
                    typeof(RequestBehaviorA<,>),
                    typeof(RequestBehaviorB<,>),
                    typeof(RequestBehaviorC<,>));

            CreateItem.Command command = new("Test Item");

            // Act
            Result<Guid> result =
                await runtime.SendAsync(provider, command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            recorder.Events.Should().Equal(
                "RequestA.Before",
                "RequestB.Before",
                "RequestC.Before",
                "RequestC.After",
                "RequestB.After",
                "RequestA.After");
        }
    }
}
