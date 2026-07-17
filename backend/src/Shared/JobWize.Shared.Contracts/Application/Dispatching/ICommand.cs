using JobWize.Runtime.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Contracts.Application.Dispatching
{
    public interface ICommand<T> : IRequest<T>
    {
    }
}
