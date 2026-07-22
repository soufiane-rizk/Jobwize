using JobWize.Runtime.Contracts.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Contracts.Modules
{
    public sealed class RuntimeOptions
    {
        internal List<IModule> Modules { get; } = [];
        internal List<Type> PipelineBehaviors { get; } = [];

        public RuntimeOptions AddModule(IModule module)
        {
            Modules.Add(module);

            return this;
        }

        public RuntimeOptions AddPipeline<TBehavior>()
        {
            PipelineBehaviors.Add(typeof(TBehavior));

            return this;
        }
    }
}
