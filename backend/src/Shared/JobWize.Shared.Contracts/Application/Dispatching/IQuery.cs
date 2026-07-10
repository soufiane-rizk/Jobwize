using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Contracts.Application.Dispatching
{
    public interface IQuery<T> : IRequest<T>
    {
    }
}
