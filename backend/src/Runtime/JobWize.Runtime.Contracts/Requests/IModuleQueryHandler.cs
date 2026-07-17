using System;
using System.Collections.Generic;
using System.Text;



namespace JobWize.Runtime.Contracts.Requests
{
    public interface IModuleQueryHandler<TModuleQuery, TResponse> 
        where TModuleQuery : IModuleQuery<TResponse>
    {
        public Task<TResponse> Handle(TModuleQuery query, CancellationToken cancellationToken);
    }
}
