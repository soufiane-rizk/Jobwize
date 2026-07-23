using FluentAssertions;
using JobWize.ModuleOne.Contracts;
using JobWize.ModuleOne.Features;
using JobWize.Runtime.Execution;
using JobWize.Runtime.Registration;
using JobWize.Runtime.UnitTests.Helpers;
using JobWize.Shared.Runtime.Contracts;

namespace JobWize.Runtime.UnitTests.Registration;

public sealed class ModuleRegistryTests
{
    private readonly ModuleRuntime _runtime;
    private readonly ModuleRegistry _registry;

    public ModuleRegistryTests()
    {
        _runtime = RuntimeTestFactory.CreateModuleOneRuntime();
        _registry = new ModuleRegistry([_runtime]);
    }

    [Fact]
    public void Resolve_Should_Return_Runtime_For_Request()
    {
        // Act
        IModuleRuntime runtime = _registry.Resolve(typeof(CreateItem.Command));

        // Assert
        runtime.Should().BeSameAs(_runtime);
    }

    [Fact]
    public void Resolve_Should_Throw_When_Request_Is_Not_Registered()
    {
        // Act
        Action action = () => _registry.Resolve(typeof(UnknownCommand));

        // Assert
        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("No runtime found for request*");
    }

    [Fact]
    public void Constructor_Should_Throw_When_Request_Is_Registered_Twice()
    {
        // Act
        Action action = () => new ModuleRegistry([_runtime, _runtime]);

        // Assert
        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Request*is already registered.");
    }

    private sealed record UnknownCommand : ICommand<Guid>;
}