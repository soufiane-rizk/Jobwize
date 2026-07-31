using JobWize.Runtime.Contracts.DependencyInjection;
using JobWize.Runtime.Contracts.Pipelines;
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

        public RuntimeOptions AddPipeline(Type behaviorType)
        {
            ArgumentNullException.ThrowIfNull(behaviorType);

            if (!behaviorType.IsGenericTypeDefinition)
            {
                throw new ArgumentException(
                    $"Pipeline behavior '{behaviorType.Name}' must be an open generic type.",
                    nameof(behaviorType));
            }

            Type pipelineBehavior = typeof(IPipelineBehavior<,>);

            bool implementsPipeline = behaviorType
                .GetInterfaces()
                .Any(i => i.IsGenericType &&
                          i.GetGenericTypeDefinition() == pipelineBehavior);

            if (!implementsPipeline)
            {
                throw new ArgumentException(
                    $"Pipeline behavior '{behaviorType.Name}' must implement {pipelineBehavior.Name}.",
                    nameof(behaviorType));
            }

            _pipelineBehaviors.Add(behaviorType);

            return this;
        }
    }
}
