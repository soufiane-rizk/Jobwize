using JobWize.Shared.Application.Results;
using JobWize.Shared.Contracts.Application.Dispatching;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Application.Dispatching
{
    public interface ICommandHandler<TCommand, TResponse> 
        : IRequestHandler<TCommand, TResponse> 
        where TCommand : ICommand<TResponse> 
        where TResponse: Result
    {
    }
}
