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
    private readonly FakeExecutionModel _executionModel;

    private readonly Dispatcher _dispatcher;

    public DispatcherTests()
    {
        _executionModel = new FakeExecutionModel();

        _dispatcher = new Dispatcher(_executionModel);
    }

    [Fact]
    public async Task SendAsync_Should_Delegate_Request_To_ExecutionModel()
    {
        // Arrange
        Result<Guid> expected = Result<Guid>.Success(Guid.NewGuid());

        _executionModel.Response = expected;

        CreateItem.Command command = new("Phone");

        // Act
        Result<Guid> actual = await _dispatcher.SendAsync(command);

        // Assert
        _executionModel.Request.Should().BeSameAs(command);

        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task PublishAsync_Should_Delegate_To_ExecutionModel()
    {
        // Arrange
        ItemCreated notification = new(Guid.NewGuid());

        // Act
        await _dispatcher.PublishAsync(notification);

        // Assert
        _executionModel.Notification.Should().BeSameAs(notification);
    }

    [Fact]
    public async Task SendModuleQueryAsync_Should_Delegate_To_ExecutionModel()
    {
        // Arrange
        GetItemSummary.Response expected = new(Guid.NewGuid(), "Phone");

        _executionModel.Response = expected;

        GetItemSummary.Query query = new(Guid.NewGuid());

        // Act
        GetItemSummary.Response actual = await _dispatcher.SendModuleQueryAsync(query);

        // Assert
        _executionModel.ModuleQuery.Should().BeSameAs(query);

        actual.Should().BeSameAs(expected);
    }
}