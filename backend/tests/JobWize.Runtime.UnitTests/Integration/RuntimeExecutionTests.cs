using FluentAssertions;
using JobWize.ModuleOne;
using JobWize.ModuleOne.Contracts;
using JobWize.ModuleOne.Features;
using JobWize.ModuleTwo;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Execution;
using JobWize.Runtime.Registration;
using JobWize.Runtime.UnitTests.Helpers;
using JobWize.Shared.Application.Results;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Integration
{
    public sealed class RuntimeExecutionTests
    {
        [Fact]
        public async Task SendAsync_Should_Execute_Nested_Notifications_In_Separate_Waves()
        {
            // Arrange
            ServiceProvider provider = RuntimeIntegrationFactory.Create();

            IServiceScope scope = provider.CreateScope();

            IDispatcher dispatcher =
                scope.ServiceProvider.GetRequiredService<IDispatcher>();

            IModuleOneNotificationStore moduleOneStore =
                scope.ServiceProvider.GetRequiredService<IModuleOneNotificationStore>();

            IModuleTwoNotificationStore moduleTwoStore =
                scope.ServiceProvider.GetRequiredService<IModuleTwoNotificationStore>();

            // Act
            Result<Guid> result =
                await dispatcher.SendAsync(new CreateItem.Command("Phone"));

            // Assert
            result.IsSuccess.Should().BeTrue();

            Guid id = result.Value;

            moduleOneStore.Published
                .Should()
                .ContainSingle()
                .Which
                .Should()
                .Be(id);

            moduleTwoStore.ItemCreatedReceived
                .Should()
                .ContainSingle()
                .Which
                .Should()
                .Be(id);

            moduleTwoStore.ItemIndexedReceived
                .Should()
                .ContainSingle()
                .Which
                .Should()
                .Be(id);
        }
    }
}
