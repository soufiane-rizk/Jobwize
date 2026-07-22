using JobWize.Runtime.Execution;
using JobWize.Runtime.Registration;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers
{
    internal sealed class FakeModuleRegistry : IModuleRegistry
    {
        public IModuleRuntime Runtime { get; set; } = default!;

        public IReadOnlyCollection<IModuleRuntime> NotificationRuntimes { get; set; } = [];

        public Type? ResolvedType { get; private set; }

        public Type? NotificationType { get; private set; }

        public IModuleRuntime Resolve(Type requestType)
        {
            ResolvedType = requestType;

            return Runtime;
        }

        public IReadOnlyCollection<IModuleRuntime> ResolveNotification(Type notificationType)
        {
            NotificationType = notificationType;

            return NotificationRuntimes;
        }
    }
}
