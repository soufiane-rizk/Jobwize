using FluentAssertions;
using JobWize.ModuleOne.Features;
using JobWize.Runtime.Pipelines;
using JobWize.Runtime.UnitTests.Helpers.Pipeline;
using JobWize.Shared.Application.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Pipelines
{
    public sealed class PipelineTypeResolverTests
    {
        [Fact]
        public void TryClose_Should_Close_Command_Behavior_For_Command()
        {
            // Arrange
            Type behavior = typeof(RecordingCommandBehavior<,>);
            Type request = typeof(CreateItem.Command);

            // Act
            Type? result = PipelineTypeResolver.TryClose(behavior, request);

            // Assert
            result.Should().NotBeNull();

            result!.GetGenericTypeDefinition()
                .Should().Be(typeof(RecordingCommandBehavior<,>));

            result.GenericTypeArguments[0]
                .Should().Be(typeof(CreateItem.Command));

            result.GenericTypeArguments[1]
                .Should().Be(typeof(Guid));
        }

        [Fact]
        public void TryClose_Should_Close_Query_Behavior_For_Query()
        {
            // Arrange
            Type behavior = typeof(RecordingQueryBehavior<,>);
            Type request = typeof(ModuleOne.Features.GetItem.Query);

            // Act
            Type? result = PipelineTypeResolver.TryClose(behavior, request);

            // Assert
            result.Should().NotBeNull();

            result!.GetGenericTypeDefinition()
                .Should().Be(typeof(RecordingQueryBehavior<,>));

            result.GenericTypeArguments[0]
                .Should().Be(typeof(ModuleOne.Features.GetItem.Query));

            result.GenericTypeArguments[1]
                .Should().Be(typeof(ModuleOne.Contracts.GetItem.Response));
        }

        [Fact]
        public void TryClose_Should_Return_Null_When_Command_Behavior_Is_Given_A_Query()
        {
            // Arrange
            Type behavior = typeof(RecordingCommandBehavior<,>);
            Type request = typeof(ModuleOne.Features.GetItem.Query);

            // Act
            Type? result = PipelineTypeResolver.TryClose(behavior, request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void TryClose_Should_Return_Null_When_Query_Behavior_Is_Given_A_Command()
        {
            // Arrange
            Type behavior = typeof(RecordingQueryBehavior<,>);
            Type request = typeof(CreateItem.Command);

            // Act
            Type? result = PipelineTypeResolver.TryClose(behavior, request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void TryClose_Should_Close_Request_Behavior_For_Command()
        {
            // Arrange
            Type behavior = typeof(RecordingRequestBehavior<,>);
            Type request = typeof(CreateItem.Command);

            // Act
            Type? result = PipelineTypeResolver.TryClose(behavior, request);

            // Assert
            result.Should().NotBeNull();

            result!.GetGenericTypeDefinition()
                .Should().Be(typeof(RecordingRequestBehavior<,>));

            result.GenericTypeArguments[0]
                .Should().Be(typeof(CreateItem.Command));

            result.GenericTypeArguments[1]
                .Should().Be(typeof(Result<Guid>));
        }
    }
}
