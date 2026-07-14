using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Contracts.Application.Dispatching
{
    public interface IRequest
    {
    }

    public interface IRequest<T> : IRequest
    {
    }
}
