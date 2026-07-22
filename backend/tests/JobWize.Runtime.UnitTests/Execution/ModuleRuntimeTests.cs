using FluentAssertions;
using JobWize.ModuleOne;
using JobWize.ModuleOne.Contracts;
using JobWize.ModuleOne.Features;
using JobWize.ModuleTwo;
using JobWize.Runtime.Discovery;
using JobWize.Runtime.Execution;
using JobWize.Runtime.UnitTests.Helpers;
using JobWize.Shared.Application.Results;
using Microsoft.Extensions.DependencyInjection;

namespace JobWize.Runtime.UnitTests.Execution;

public sealed class ModuleRuntimeTests
{
    private readonly ModuleRuntime _runtime;
    private readonly ServiceProvider _provider;

    public ModuleRuntimeTests()
    {
        (_runtime, _provider) = RuntimeTestFactory.CreateModuleOneRuntimeWithProvider();
    }

    [Fact]
    public async Task SendAsync_Should_Invoke_Command_Handler()
    {
        // Arrange
        CreateItem.Command command = new("Phone");

        // Act
        Result<Guid> result = await _runtime.SendAsync(_provider, command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(ItemRepository.CreatedId);
    }

    [Fact]
    public async Task SendAsync_Should_Return_Command_Result()
    {
        // Arrange
        CreateItem.Command command = new("Phone");

        // Act
        Result<Guid> result = await _runtime.SendAsync(_provider, command, CancellationToken.None);

        // Assert
        result.Value.Should().Be(ItemRepository.CreatedId);
    }

    [Fact]
    public async Task SendAsync_Should_Invoke_Query_Handler()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        ModuleOne.Features.GetItem.Query query = new(id);

        // Act
        Result<ModuleOne.Contracts.GetItem.Response> result = await _runtime.SendAsync(_provider, query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Id.Should().Be(id);

        result.Value.Name.Should().Be("Test Item");
    }

    [Fact]
    public async Task PublishAsync_Should_Invoke_Module_Notification_Handlers()
    {
        // Arrange
        Guid itemId = Guid.NewGuid();

        ItemCreated notification = new(itemId);

        IModuleOneNotificationStore store = _provider.GetRequiredService<IModuleOneNotificationStore>();

        store.Published.Should().BeEmpty();

        await _runtime.PublishAsync(_provider, notification);

        store.Published.Should().ContainSingle().Which.Should().Be(itemId);
    }

}