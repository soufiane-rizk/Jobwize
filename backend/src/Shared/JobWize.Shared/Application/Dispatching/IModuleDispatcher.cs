using JobWize.Shared.Contracts.Application.Dispatching;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Application.Dispatching
{
    public interface IModuleDispatcher
    {
        Task<TResponse> SendAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default);
    }
}
