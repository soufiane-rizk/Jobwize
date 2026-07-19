using JobWize.Runtime.Contracts.Requests;
using JobWize.Runtime.Execution;
using JobWize.Runtime.Registration;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Dispatching
{
    public interface IModuleDispatcher
    {
        Task<TResponse> SendAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default);
    }

    public class InProcessModuleDispatcher : IModuleDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IModuleRegistry _registry;

        public InProcessModuleDispatcher(IServiceProvider serviceProvider, IModuleRegistry registry)
        {
            _serviceProvider = serviceProvider;
            _registry = registry;
        }

        public Task<TResponse> SendAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default)
        {
            IModuleRuntime runtime = _registry.Resolve(query.GetType());

            return runtime.SendAsync(_serviceProvider, query, cancellationToken);
        }
    }
}
