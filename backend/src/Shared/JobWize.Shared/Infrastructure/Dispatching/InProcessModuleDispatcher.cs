using JobWize.Shared.Application.Dispatching;
using JobWize.Shared.Contracts.Application.Dispatching;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Infrastructure.Dispatching
{
    public class InProcessModuleDispatcher : IModuleDispatcher
    {
        public Task<TResponse> SendAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
