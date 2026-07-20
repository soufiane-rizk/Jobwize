using FluentAssertions;
using JobWize.ModuleOne.Features;
using JobWize.ModuleOne.Contracts;
using JobWize.Runtime.Dispatching;
using JobWize.Runtime.UnitTests.Helpers;
using JobWize.Shared.Application.Results;
using Microsoft.Extensions.DependencyInjection;
using GetItem = JobWize.ModuleOne.Contracts.GetItem;

namespace JobWize.Runtime.UnitTests.Dispatching;

public sealed class DispatcherTests
{
    private readonly FakeModuleRuntime _runtime;
    private readonly FakeModuleRegistry _registry;
    private readonly FakeNotificationContext _notificationContext;
    private readonly FakeModuleDispatcher _moduleDispatcher;
    private readonly IServiceProvider _serviceProvider;
    private readonly FakeNotificationDispatcher _notificationDispatcher;

    private readonly Dispatcher _dispatcher;

    public DispatcherTests()
    {
        _runtime = new FakeModuleRuntime();
        _registry = new FakeModuleRegistry
        {
            Runtime = _runtime
        };

        _notificationContext = new FakeNotificationContext();
        _moduleDispatcher = new FakeModuleDispatcher();
        _notificationDispatcher = new FakeNotificationDispatcher();

        _serviceProvider = new ServiceCollection().BuildServiceProvider();

        _dispatcher = new Dispatcher(
            _moduleDispatcher,
            _notificationContext,
            _serviceProvider,
            _registry,
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
        await _dispatcher.SendAsync(command);

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
        await _dispatcher.SendAsync(command);

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
        Result<Guid> actual = await _dispatcher.SendAsync(command);

        // Assert
        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task PublishAsync_Should_Collect_Notification()
    {
        // Arrange
        ItemCreated notification = new(Guid.NewGuid());

        // Act
        await _dispatcher.PublishAsync(notification);

        // Assert
        _notificationContext.BeginCalled.Should().BeTrue();

        _notificationContext.PublishCalled.Should().BeTrue();

        _notificationContext.PublishedNotification.Should().BeSameAs(notification);
    }

    [Fact]
    public async Task PublishAsync_Should_Invoke_Runtime()
    {
        // Arrange
        ItemCreated notification = new(Guid.NewGuid());

        // Act
        await _dispatcher.PublishAsync(notification);

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
        await _dispatcher.PublishAsync(notification);

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
        await _dispatcher.PublishAsync(notification);

        // Assert
        _notificationContext.CompleteCalled.Should().BeFalse();
    }

    [Fact]
    public async Task SendModuleQueryAsync_Should_Delegate_To_ModuleDispatcher()
    {
        // Arrange
        GetItemSummary.Response expected = new(Guid.NewGuid(), "Phone");

        _moduleDispatcher.Response = expected;

        GetItemSummary.Query query = new(Guid.NewGuid());

        // Act
        GetItemSummary.Response actual = await _dispatcher.SendModuleQueryAsync(query);

        // Assert
        _moduleDispatcher.Query.Should().BeSameAs(query);
        actual.Should().BeSameAs(expected);
    }
}