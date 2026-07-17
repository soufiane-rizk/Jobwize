using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Contracts.Application.Dispatching;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Runtime.Handlers
{
    public interface ICommandHandler<TCommand, TValue> : IRequestHandler<TCommand, Result<TValue>>
        where TCommand : ICommand<TValue>
    {
    }
}
