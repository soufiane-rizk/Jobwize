using JobWize.Runtime.Execution;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Registration
{
    public interface IModuleRegistry
    {
        ModuleRuntime Resolve(Type requestType);
    }

    public sealed class ModuleRegistry : IModuleRegistry
    {
        private readonly IReadOnlyDictionary<Type, ModuleRuntime> _runtimes;

        public ModuleRegistry(IEnumerable<ModuleRuntime> runtimes)
        {
            Dictionary<Type, ModuleRuntime> dictionary = [];

            foreach (ModuleRuntime runtime in runtimes)
            {
                foreach (Type requestType in runtime.RequestTypes)
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

        public ModuleRuntime Resolve(Type requestType)
        {
            if (!_runtimes.TryGetValue(requestType, out ModuleRuntime? runtime))
            {
                throw new InvalidOperationException(
                    $"No runtime found for request '{requestType.FullName}'.");
            }

            return runtime;
        }
    }
}
