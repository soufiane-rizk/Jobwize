using FluentAssertions;

using JobWize.ModuleOne;
using JobWize.ModuleOne.Contracts;
using JobWize.ModuleOne.Features;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Execution;
using JobWize.Runtime.Registration;
using JobWize.Runtime.UnitTests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobWize.Runtime.UnitTests.Registration;

public sealed class RuntimeBuilderTests
{
    [Fact]
    public void Build_Should_Create_Runtime_For_Module()
    {
        // Arrange
        RuntimeBuilder builder = new();

        ModuleOneModule module = new();

        ServiceCollection services = [];

        // Act
        ModuleRuntime runtime = builder.Build(module, services);

        // Assert
        runtime.Should().NotBeNull();

        runtime.Name.Should().Be("ModuleOne");
    }

    [Fact]
    public void Build_Should_Discover_Request()
    {
        // Arrange
        RuntimeBuilder builder = new();

        ModuleOneModule module = new();

        ServiceCollection services = [];

        // Act
        ModuleRuntime runtime = builder.Build(module, services);

        // Assert
        runtime.DispatchableTypes.Should().Contain(typeof(CreateItem.Command));
    }

    [Fact]
    public void Build_Should_Create_HandlerDescriptor()
    {
        // Arrange
        RuntimeBuilder builder = new();

        ModuleOneModule module = new();

        ServiceCollection services = [];

        // Act
        ModuleRuntime runtime = builder.Build(module, services);

        // Assert
        runtime.Descriptor.Handlers.Should()
            .ContainSingle(handler =>
                handler.HandlerType == typeof(ModuleOne.Features.CreateItem.Handler));
    }

    [Fact]
    public void Build_Should_Register_Command_Handler()
    {
        // Arrange
        RuntimeBuilder builder = new();

        ModuleOneModule module = new();

        ServiceCollection services = [];

        services.AddSingleton<IDispatcher, FakeDispatcher>();

        IConfiguration configuration = new ConfigurationBuilder().Build();

        module.ConfigureServices(services, configuration);

        // Act
        builder.Build(module, services);

        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<ModuleOne.Features.CreateItem.Handler>().Should().NotBeNull();
    }
}