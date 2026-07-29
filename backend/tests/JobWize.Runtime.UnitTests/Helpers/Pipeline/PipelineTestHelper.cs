using JobWize.Runtime.Contracts.Modules;
using JobWize.Runtime.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers.Pipeline
{
    internal static class PipelineTestHelper
    {
        public static PipelineResolver CreateResolver(params Type[] pipelines)
        {
            RuntimeOptions options = new();

            foreach (Type pipeline in pipelines)
            {
                options.AddPipeline(pipeline);
            }

            return new PipelineResolver(options);
        }

        public static ServiceProvider CreateProvider(Action<IServiceCollection>? configure = null)
        {
            ServiceCollection services = [];

            configure?.Invoke(services);

            return services.BuildServiceProvider();
        }
    }
}
