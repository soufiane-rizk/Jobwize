using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Runtime.Contracts
{
    public interface ICommand<T> : IRequest<Result<T>>
    {
    }
}
