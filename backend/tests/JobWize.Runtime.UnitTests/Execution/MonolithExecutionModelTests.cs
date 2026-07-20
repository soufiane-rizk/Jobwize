using FluentAssertions;
using JobWize.ModuleOne.Contracts;
using JobWize.ModuleOne.Features;
using JobWize.Runtime.Execution;
using JobWize.Runtime.UnitTests.Helpers;
using JobWize.Shared.Application.Results;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Execution
{
    public sealed class MonolithExecutionModelTests
    {
        private readonly FakeModuleRuntime _runtime;
        private readonly FakeModuleRegistry _registry;
        private readonly FakeNotificationContext _notificationContext;
        private readonly FakeNotificationDispatcher _notificationDispatcher;
        private readonly IServiceProvider _serviceProvider;

        private readonly MonolithExecutionModel _executionModel;

        public MonolithExecutionModelTests()
        {
            _runtime = new FakeModuleRuntime();

            _registry = new FakeModuleRegistry
            {
                Runtime = _runtime
            };

            _notificationContext = new FakeNotificationContext();

            _notificationDispatcher = new FakeNotificationDispatcher();

            _serviceProvider = new ServiceCollection().BuildServiceProvider();

            _executionModel = new MonolithExecutionModel(
                _serviceProvider,
                _registry,
                _notificationContext,
                _notificationDispatcher);
        }

        [Fact]
        public async Task SendAsync_Should_Resolve_Runtime_For_Request()
        {
            // Arrange
            Result<Guid> expected = Result<Guid>.Success(Guid.NewGuid());

            _runtime.Response = expected;

            CreateItem.Command command = new("Phone");

            // Act
            await _executionModel.SendAsync(command);

            // Assert
            _registry.ResolvedType.Should().Be(typeof(CreateItem.Command));

            _runtime.SendCalled.Should().BeTrue();
        }

        [Fact]
        public async Task SendAsync_Should_Forward_Request_To_Runtime()
        {
            // Arrange
            Result<Guid> expected = Result<Guid>.Success(Guid.NewGuid());

            _runtime.Response = expected;

            CreateItem.Command command = new("Phone");

            // Act
            await _executionModel.SendAsync(command);

            // Assert
            _runtime.Request.Should().BeSameAs(command);

            _runtime.ServiceProvider.Should().BeSameAs(_serviceProvider);
        }

        [Fact]
        public async Task SendAsync_Should_Return_Runtime_Response()
        {
            // Arrange
            Result<Guid> expected = Result<Guid>.Success(Guid.NewGuid());

            _runtime.Response = expected;

            CreateItem.Command command = new("Phone");

            // Act
            Result<Guid> actual = await _executionModel.SendAsync(command);

            // Assert
            actual.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task SendAsync_Should_Resolve_Runtime_For_ModuleQuery()
        {
            // Arrange
            GetItemSummary.Response expected = new(Guid.NewGuid(), "Phone");

            _runtime.Response = expected;

            GetItemSummary.Query query = new(Guid.NewGuid());

            // Act
            await _executionModel.SendAsync(query);

            // Assert
            _registry.ResolvedType.Should().Be(typeof(GetItemSummary.Query));

            _runtime.SendCalled.Should().BeTrue();
        }

        [Fact]
        public async Task SendAsync_Should_Forward_ModuleQuery_To_Runtime()
        {
            // Arrange
            GetItemSummary.Response expected = new(Guid.NewGuid(), "Phone");

            _runtime.Response = expected;

            GetItemSummary.Query query = new(Guid.NewGuid());

            // Act
            await _executionModel.SendAsync(query);

            // Assert
            _runtime.ModuleQuery.Should().BeSameAs(query);

            _runtime.ServiceProvider.Should().BeSameAs(_serviceProvider);
        }

        [Fact]
        public async Task SendAsync_Should_Return_ModuleQuery_Response()
        {
            // Arrange
            GetItemSummary.Response expected = new(Guid.NewGuid(), "Phone");

            _runtime.Response = expected;

            GetItemSummary.Query query = new(Guid.NewGuid());

            // Act
            GetItemSummary.Response actual = await _executionModel.SendAsync(query);

            // Assert
            actual.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task PublishAsync_Should_Begin_Notification_Context()
        {
            // Arrange
            ItemCreated notification = new(Guid.NewGuid());

            // Act
            await _executionModel.PublishAsync(notification);

            // Assert
            _notificationContext.BeginCalled.Should().BeTrue();
        }

        [Fact]
        public async Task PublishAsync_Should_Publish_Notification()
        {
            // Arrange
            ItemCreated notification = new(Guid.NewGuid());

            // Act
            await _executionModel.PublishAsync(notification);

            // Assert
            _notificationContext.PublishCalled.Should().BeTrue();

            _notificationContext.PublishedNotification.Should().BeSameAs(notification);
        }

        [Fact]
        public async Task PublishAsync_Should_Invoke_Runtime()
        {
            // Arrange
            ItemCreated notification = new(Guid.NewGuid());

            // Act
            await _executionModel.PublishAsync(notification);

            // Assert
            _registry.ResolvedType.Should().Be(typeof(ItemCreated));

            _runtime.PublishCalled.Should().BeTrue();

            _runtime.Notification.Should().BeSameAs(notification);

            _runtime.ServiceProvider.Should().BeSameAs(_serviceProvider);
        }

        [Fact]
        public async Task PublishAsync_Should_Complete_Context_For_Root_Publication()
        {
            // Arrange
            _notificationContext.BeginResult = true;

            ItemCreated notification = new(Guid.NewGuid());

            // Act
            await _executionModel.PublishAsync(notification);

            // Assert
            _notificationContext.CompleteCalled.Should().BeTrue();
        }

        [Fact]
        public async Task PublishAsync_Should_Not_Complete_Context_For_Nested_Publication()
        {
            // Arrange
            _notificationContext.BeginResult = false;

            ItemCreated notification = new(Guid.NewGuid());

            // Act
            await _executionModel.PublishAsync(notification);

            // Assert
            _notificationContext.CompleteCalled.Should().BeFalse();
        }

        [Fact]
        public async Task PublishAsync_Should_Dispatch_Current_Wave_For_Root_Publication()
        {
            // Arrange
            _notificationContext.BeginResult = true;

            ItemCreated notification = new(Guid.NewGuid());

            // Act
            await _executionModel.PublishAsync(notification);

            // Assert
            _notificationDispatcher.DispatchCalled.Should().BeTrue();

            _notificationDispatcher.Notifications.Should().ContainSingle();

            _notificationDispatcher.Notifications.Single().Should().BeSameAs(notification);
        }

        [Fact]
        public async Task PublishAsync_Should_Not_Dispatch_Current_Wave_For_Nested_Publication()
        {
            // Arrange
            _notificationContext.BeginResult = false;

            ItemCreated notification = new(Guid.NewGuid());

            // Act
            await _executionModel.PublishAsync(notification);

            // Assert
            _notificationDispatcher.DispatchCalled.Should().BeFalse();
        }
    }
}
