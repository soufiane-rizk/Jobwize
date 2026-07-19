using JobWize.Runtime.Execution;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Registration
{
    public interface IModuleRegistry
    {
        IModuleRuntime Resolve(Type requestType);
    }

    public sealed class ModuleRegistry : IModuleRegistry
    {
        private readonly IReadOnlyDictionary<Type, IModuleRuntime> _runtimes;

        public ModuleRegistry(IEnumerable<IModuleRuntime> runtimes)
        {
            Dictionary<Type, IModuleRuntime> dictionary = [];

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
            }

            _runtimes = dictionary;
        }

        public IModuleRuntime Resolve(Type requestType)
        {
            if (!_runtimes.TryGetValue(requestType, out IModuleRuntime? runtime))
            {
                throw new InvalidOperationException(
                    $"No runtime found for request '{requestType.FullName}'.");
            }

            return runtime;
        }
    }
}
