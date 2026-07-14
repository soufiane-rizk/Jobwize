using System;
using System.Collections.Generic;
using System.Text;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Contracts.Application.Dispatching;


namespace JobWize.Shared.Runtime.Handlers
{
    public interface IModuleQueryHandler<TModuleQuery, TResponse> 
        where TModuleQuery : IModuleQuery<TResponse>
        where TResponse : Result
    {
        public Task<TResponse> Handle(TModuleQuery request, CancellationToken cancellationToken);
    }
}
