using JobWize.Runtime.Execution;
using JobWize.Runtime.Registration;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers
{
    internal sealed class FakeModuleRegistry : IModuleRegistry
    {
        public IModuleRuntime Runtime { get; set; } = null!;

        public Type? ResolvedType { get; private set; }

        public IModuleRuntime Resolve(Type requestType)
        {
            ResolvedType = requestType;

            return Runtime;
        }
    }
}
