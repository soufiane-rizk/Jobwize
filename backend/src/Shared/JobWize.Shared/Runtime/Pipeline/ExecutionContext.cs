using JobWize.Shared.Contracts.Application.Dispatching;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Runtime.Pipeline
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
            ServiceProvider = serviceProvider;
            CancellationToken = cancellationToken;
        }
    }
}
