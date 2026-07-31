using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Runtime.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers.Pipeline
{
    public sealed class DependencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    {
        public RecordingDependency Dependency { get; }

        public DependencyBehavior(RecordingDependency dependency)
        {
            Dependency = dependency;
        }

        public Task<TResponse> HandleAsync(
            ExecutionContext<TRequest, TResponse> context,
            RequestExecutionDelegate<TResponse> next)
        {
            return next();
        }
    }
}
