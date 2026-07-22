using JobWize.Runtime.Contracts.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers
{
    internal sealed class ThrowingModuleRuntime : FakeModuleRuntime
    {
        public override Task PublishAsync(IServiceProvider serviceProvider, INotification notification, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Boom");
        }
    }
}
