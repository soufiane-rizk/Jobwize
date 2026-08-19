using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Application.Security
{
    public interface IUserContext
    {
        Guid UserId { get; }
    }
}
