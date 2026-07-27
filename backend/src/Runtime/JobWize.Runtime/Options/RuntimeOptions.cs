using JobWize.Runtime.Contracts.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace JobWize.Runtime.Contracts.Modules
{
    public sealed class RuntimeOptions
    {
        private readonly List<Type> _pipelineBehaviors = [];
        private readonly List<IModule> _modules = [];

        internal IReadOnlyCollection<IModule> Modules => _modules;
        internal IReadOnlyCollection<Type> PipelineBehaviors => _pipelineBehaviors;

        public RuntimeOptions AddModule(IModule module)
        {
            _modules.Add(module);

            return this;
        }

        public RuntimeOptions AddPipeline<TBehavior>()
        {
            _pipelineBehaviors.Add(typeof(TBehavior));

            return this;
        }
    }
}
