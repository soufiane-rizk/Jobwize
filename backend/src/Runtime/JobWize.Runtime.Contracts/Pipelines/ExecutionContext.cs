using JobWize.Runtime.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Contracts.Pipelines
{
    public sealed class ExecutionContext<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public TRequest Request { get; }
        public IServiceProvider ServiceProvider { get; }

        public CancellationToken CancellationToken { get; }

        public ExecutionContext(TRequest request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            Request = request;
            CancellationToken = cancellationToken;
            ServiceProvider = serviceProvider;
        }
    }
}
