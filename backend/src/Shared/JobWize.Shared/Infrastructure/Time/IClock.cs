using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Infrastructure.Time
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}
