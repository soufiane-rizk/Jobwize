using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Contracts.Application.Dispatching;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Runtime.Handlers
{
    public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
        where TResponse: Result
    {
    }
}
