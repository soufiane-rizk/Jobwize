using JobWize.Shared.Application.Results;
using JobWize.Shared.Contracts.Application.Dispatching;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Application.Dispatching
{
    public interface IQueryHandler<TQuery, TResponse> : 
        IRequestHandler<TQuery, TResponse> 
        where TQuery : IQuery<TResponse>
        where TResponse: Result
    {
    }
}
