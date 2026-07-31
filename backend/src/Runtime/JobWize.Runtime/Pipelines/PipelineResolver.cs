using JobWize.Runtime.Contracts.Modules;
using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Runtime.Contracts.Requests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Pipelines
{
    internal interface IPipelineResolver
    {
        IReadOnlyCollection<IPipelineBehavior<TRequest, TResponse>> Resolve<TRequest, TResponse>(IServiceProvider serviceProvider)
            where TRequest : IRequest<TResponse>;
    }

    internal sealed class PipelineResolver : IPipelineResolver
    {
        private readonly RuntimeOptions _options;

        public PipelineResolver(RuntimeOptions options)
        {
            _options = options;
        }

        public IReadOnlyCollection<IPipelineBehavior<TRequest, TResponse>> Resolve<TRequest, TResponse>(
            IServiceProvider serviceProvider)
            where TRequest : IRequest<TResponse>
        {
            List<IPipelineBehavior<TRequest, TResponse>> behaviors = [];

            foreach (Type pipelineType in _options.PipelineBehaviors)
            {
                Type? closedType = PipelineTypeResolver.TryClose(pipelineType, typeof(TRequest));

                if (closedType is null)
                {
                    continue;
                }

                if (!typeof(IPipelineBehavior<TRequest, TResponse>).IsAssignableFrom(closedType))
                {
                    continue;
                }

                IPipelineBehavior<TRequest, TResponse> behavior =
                    (IPipelineBehavior<TRequest, TResponse>)ActivatorUtilities.CreateInstance(
                        serviceProvider,
                        closedType);

                behaviors.Add(behavior);
            }

            return behaviors;
        }
    }
}
