using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Runtime.Contracts
{
    public interface IQuery<T> : IRequest<Result<T>>
    {
    }
}
