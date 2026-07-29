using FluentAssertions;
using JobWize.ModuleOne.Features;
using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Runtime.Pipelines;
using JobWize.Runtime.UnitTests.Helpers.Pipeline;
using JobWize.Shared.Application.Results;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Pipelines
{
    public sealed class PipelineResolverTests
    {
        [Fact]
        public void Resolve_Should_Return_Command_Behavior()
        {
            // Arrange
            PipelineResolver resolver =
                PipelineTestHelper.CreateResolver(typeof(RecordingCommandBehavior<,>));

            ServiceProvider provider = PipelineTestHelper.CreateProvider(services =>
            {
                services.AddSingleton(new PipelineExecutionRecorder());
            });

            // Act
            IReadOnlyCollection<IPipelineBehavior<CreateItem.Command, Result<Guid>>> behaviors =
                resolver.Resolve<CreateItem.Command, Result<Guid>>(provider);

            // Assert
            behaviors.Should().ContainSingle();

            behaviors.Single()
                .Should()
                .BeOfType<RecordingCommandBehavior<CreateItem.Command, Guid>>();
        }

        [Fact]
        public void Resolve_Should_Ignore_Query_Behavior_For_Command()
        {
            // Arrange
            PipelineResolver resolver =
                PipelineTestHelper.CreateResolver(typeof(RecordingQueryBehavior<,>));

            ServiceProvider provider = PipelineTestHelper.CreateProvider(services =>
            {
                services.AddSingleton(new PipelineExecutionRecorder());
            });

            // Act
            IReadOnlyCollection<IPipelineBehavior<CreateItem.Command, Result<Guid>>> behaviors =
                resolver.Resolve<CreateItem.Command, Result<Guid>>(provider);

            // Assert
            behaviors.Should().BeEmpty();
        }

        [Fact]
        public void Resolve_Should_Return_All_Matching_Behaviors()
        {
            // Arrange
            PipelineResolver resolver =
                PipelineTestHelper.CreateResolver(
                    typeof(RequestBehaviorA<,>),
                    typeof(RequestBehaviorB<,>),
                    typeof(RequestBehaviorC<,>));

            ServiceProvider provider = PipelineTestHelper.CreateProvider(services =>
            {
                services.AddSingleton(new PipelineExecutionRecorder());
            });

            // Act
            IReadOnlyCollection<IPipelineBehavior<CreateItem.Command, Result<Guid>>> behaviors =
                resolver.Resolve<CreateItem.Command, Result<Guid>>(provider);

            // Assert
            behaviors.Should().HaveCount(3);
        }

        [Fact]
        public void Resolve_Should_Preserve_Registration_Order()
        {
            // Arrange
            PipelineResolver resolver =
                PipelineTestHelper.CreateResolver(
                    typeof(RequestBehaviorA<,>),
                    typeof(RequestBehaviorB<,>),
                    typeof(RequestBehaviorC<,>));

            ServiceProvider provider = PipelineTestHelper.CreateProvider(services =>
            {
                services.AddSingleton(new PipelineExecutionRecorder());
            });

            // Act
            IReadOnlyCollection<IPipelineBehavior<CreateItem.Command, Result<Guid>>> behaviors =
                resolver.Resolve<CreateItem.Command, Result<Guid>>(provider);

            // Assert
            behaviors.Select(x => x.GetType()).Should().Equal(
                typeof(RequestBehaviorA<CreateItem.Command, Result<Guid>>),
                typeof(RequestBehaviorB<CreateItem.Command, Result<Guid>>),
                typeof(RequestBehaviorC<CreateItem.Command, Result<Guid>>));
        }

        [Fact]
        public void Resolve_Should_Create_Behavior_Using_Dependency_Injection()
        {
            // Arrange
            PipelineResolver resolver =
                PipelineTestHelper.CreateResolver(typeof(DependencyBehavior<,>));

            RecordingDependency dependency = new();

            ServiceProvider provider = PipelineTestHelper.CreateProvider(services =>
            {
                services.AddSingleton(dependency);
            });

            // Act
            DependencyBehavior<CreateItem.Command, Result<Guid>> behavior =
                resolver.Resolve<CreateItem.Command, Result<Guid>>(provider)
                    .Single()
                    .Should()
                    .BeOfType<DependencyBehavior<CreateItem.Command, Result<Guid>>>()
                    .Subject;

            // Assert
            behavior.Dependency.Should().BeSameAs(dependency);
        }
    }
}
