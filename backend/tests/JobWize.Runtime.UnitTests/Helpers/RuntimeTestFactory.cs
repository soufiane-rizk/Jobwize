using JobWize.ModuleOne;
using JobWize.Runtime.Execution;
using JobWize.Runtime.Registration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobWize.Runtime.UnitTests.Helpers;

internal static class RuntimeTestFactory
{
    private static (ModuleRuntime Runtime, ServiceProvider Provider) Create()
    {
        ModuleOneModule module = new();

        ServiceCollection services = [];
        IConfiguration configuration = new ConfigurationBuilder().Build();

        module.ConfigureServices(services, configuration);

        RuntimeBuilder builder = new();

        ModuleRuntime runtime = builder.Build(module, services);

        ServiceProvider provider = services.BuildServiceProvider();

        return (runtime, provider);
    }

    public static ModuleRuntime CreateModuleOneRuntime()
    {
        return Create().Runtime;
    }

    public static (ModuleRuntime Runtime, ServiceProvider Provider) CreateModuleOneRuntimeWithProvider()
    {
        return Create();
    }
}