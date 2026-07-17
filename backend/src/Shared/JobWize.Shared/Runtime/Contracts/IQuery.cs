using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Contracts.Application.Dispatching
{
    public interface IQuery<T> : IRequest<Result<T>>
    {
    }
}
