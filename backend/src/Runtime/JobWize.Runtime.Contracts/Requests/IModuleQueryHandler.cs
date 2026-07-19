using System;
using System.Collections.Generic;
using System.Text;



namespace JobWize.Runtime.Contracts.Requests
{
    public interface IModuleQueryHandler<TModuleQuery, TResponse> 
        where TModuleQuery : IModuleQuery<TResponse>
    {
        public Task<TResponse> HandleAsync(TModuleQuery query, CancellationToken cancellationToken);
    }
}
