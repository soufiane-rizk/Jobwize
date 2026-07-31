using JobWize.ModuleOne;
using JobWize.ModuleTwo;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Contracts.Modules;
using JobWize.Runtime.Execution;
using JobWize.Runtime.Pipelines;
using JobWize.Runtime.Registration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobWize.Runtime.UnitTests.Helpers;

internal static class RuntimeTestFactory
{
    private static (ModuleRuntime Runtime, ServiceProvider Provider) Create(Action<IServiceCollection>? configureServices = null, params Type[] pipelineBehaviors)
    {
        ModuleOneModule module = new();

        ServiceCollection services = [];

        RuntimeOptions options = new();

        options.AddModule(module);

        foreach (Type pipeline in pipelineBehaviors)
        {
            options.AddPipeline(pipeline);
        }

        services.AddSingleton(options);

        services.AddSingleton<IDispatcher, FakeDispatcher>();

        services.AddScoped<IPipelineResolver, PipelineResolver>();
        services.AddScoped<IPipelineExecutor, PipelineExecutor>();

        configureServices?.Invoke(services);

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

    public static (ModuleRuntime Runtime, ServiceProvider Provider) CreateModuleOneRuntimeWithProvider(Action<IServiceCollection>? configureServices = null, params Type[] pipelineBehaviors)
    {
        return Create(configureServices, pipelineBehaviors);
    }

    public static (ModuleRuntime Runtime, ServiceProvider Provider) CreateModuleOneRuntimeWithAllModules()
    {
        ModuleOneModule moduleOne = new();
        ModuleTwoModule moduleTwo = new();

        ServiceCollection services = [];
        IConfiguration configuration = new ConfigurationBuilder().Build();

        services.AddSingleton<IDispatcher, FakeDispatcher>();
        services.AddScoped<IPipelineResolver, PipelineResolver>();
        services.AddScoped<IPipelineExecutor, PipelineExecutor>();

        moduleOne.ConfigureServices(services, configuration);
        moduleTwo.ConfigureServices(services, configuration);

        RuntimeBuilder builder = new();

        ModuleRuntime moduleOneRuntime = builder.Build(moduleOne, services);

        _ = builder.Build(moduleTwo, services);

        ServiceProvider provider = services.BuildServiceProvider();

        return (moduleOneRuntime, provider);
    }
}