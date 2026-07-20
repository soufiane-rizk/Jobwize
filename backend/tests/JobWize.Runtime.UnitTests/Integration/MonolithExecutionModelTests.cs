using FluentAssertions;
using JobWize.ModuleOne;
using JobWize.ModuleOne.Contracts;
using JobWize.ModuleTwo;
using JobWize.ModuleTwo.Features;
using JobWize.ModuleTwo.Contracts;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.DependencyInjection;
using JobWize.Runtime.Dispatching;
using JobWize.Runtime.Execution;
using JobWize.Runtime.Registration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using JobWize.Shared.Application.Results;

namespace JobWize.Runtime.UnitTests.Integration
{
    public sealed class MonolithExecutionModelTests
    {
        [Fact]
        public async Task SendAsync_Should_Execute_Module_Query_Handler()
        {
            // Arrange
            ServiceCollection services = [];

            IConfiguration configuration = new ConfigurationBuilder()
                .Build();

            services.AddRuntime(
                configuration,
                options =>
                {
                    options.AddModule(new ModuleOneModule());
                });

            ServiceProvider provider = services.BuildServiceProvider();

            IExecutionModel executionModel =
                provider.GetRequiredService<IExecutionModel>();

            GetItemSummary.Query query = new(Guid.NewGuid());

            // Act
            GetItemSummary.Response response = await executionModel.SendAsync(query);

            // Assert
            response.Id.Should().Be(query.Id);

            response.Name.Should().Be("Test Item");
        }

        [Fact]
        public async Task SendAsync_Should_Execute_Cross_Module_Query()
        {
            // Arrange
            ServiceCollection services = [];

            IConfiguration configuration = new ConfigurationBuilder()
                .Build();

            services.AddRuntime(
                configuration,
                options =>
                {
                    options.AddModule(new ModuleOneModule());
                    options.AddModule(new ModuleTwoModule());
                });

            ServiceProvider provider = services.BuildServiceProvider();

            IDispatcher dispatcher = provider.GetRequiredService<IDispatcher>();

            ModuleTwo.Features.GetExternalItemSummary.Query query = new(Guid.NewGuid());

            // Act
            Result<ModuleTwo.Contracts.GetExternalItemSummary.Response> response = await dispatcher.SendAsync(query);

            // Assert
            response.Value.Id.Should().Be(query.Id);

            response.Value.Name.Should().Be("Test Item");
        }
    }
}