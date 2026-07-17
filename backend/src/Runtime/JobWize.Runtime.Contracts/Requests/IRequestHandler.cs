using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Contracts.Requests
{
    public interface IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}
