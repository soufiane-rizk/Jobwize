using JobWize.ModuleOne;
using JobWize.ModuleTwo;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Dispatching;
using JobWize.Runtime.Execution;
using JobWize.Runtime.Registration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using JobWize.Runtime.DependencyInjection;

namespace JobWize.Runtime.UnitTests.Integration
{
    internal static class RuntimeIntegrationFactory
    {
        public static ServiceProvider Create()
        {
            ServiceCollection services = [];

            IConfiguration configuration =
                new ConfigurationBuilder().Build();

            services.AddRuntime(
                configuration,
                options =>
                {
                    options.AddModule(new ModuleOneModule());
                    options.AddModule(new ModuleTwoModule());
                });

            return services.BuildServiceProvider();
        }
    }
}
