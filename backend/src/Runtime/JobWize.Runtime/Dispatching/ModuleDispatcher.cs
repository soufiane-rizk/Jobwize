using JobWize.Runtime.Contracts.Requests;
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
        public Task<TResponse> SendAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
