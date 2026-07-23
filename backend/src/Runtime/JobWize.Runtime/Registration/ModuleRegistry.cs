using JobWize.Runtime.Discovery;
using JobWize.Runtime.Execution;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Registration
{
    public interface IModuleRegistry
    {
        IModuleRuntime Resolve(Type requestType);
        IReadOnlyCollection<IModuleRuntime> ResolveNotification(Type notificationType);
    }

    public sealed class ModuleRegistry : IModuleRegistry
    {
        private readonly IReadOnlyDictionary<Type, IModuleRuntime> _dispatchableRuntimes;
        private readonly IReadOnlyDictionary<Type, IReadOnlyCollection<IModuleRuntime>> _notificationRuntimes;

        public ModuleRegistry(IEnumerable<IModuleRuntime> runtimes)
        {
            Dictionary<Type, IModuleRuntime> dictionary = [];
            Dictionary<Type, List<IModuleRuntime>> notificationDictionary = [];

            foreach (IModuleRuntime runtime in runtimes)
            {
                foreach (Type requestType in runtime.DispatchableTypes)
                {
                    if (!dictionary.TryAdd(requestType, runtime))
                    {
                        throw new InvalidOperationException(
                            $"Request '{requestType.FullName}' is already registered.");
                    }
                }

                foreach (Type notificationType in runtime.NotificationTypes)
                {
                    if (!notificationDictionary.TryGetValue(notificationType, out List<IModuleRuntime>? interestedRuntimes))
                    {
                        interestedRuntimes = [];
                        notificationDictionary.Add(notificationType, interestedRuntimes);
                    }

                    interestedRuntimes.Add(runtime);
                }
            }

            _dispatchableRuntimes = dictionary;
            _notificationRuntimes = notificationDictionary.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyCollection<IModuleRuntime>)pair.Value.ToArray());
        }

        public IModuleRuntime Resolve(Type requestType)
        {
            if (!_dispatchableRuntimes.TryGetValue(requestType, out IModuleRuntime? runtime))
            {
                throw new InvalidOperationException(
                    $"No runtime found for request '{requestType.FullName}'.");
            }

            return runtime;
        }

        public IReadOnlyCollection<IModuleRuntime> ResolveNotification(Type notificationType)
        {
            if (_notificationRuntimes.TryGetValue(notificationType, out IReadOnlyCollection<IModuleRuntime>? runtimes))
            {
                return runtimes;
            }

            return [];
        }
    }
}
