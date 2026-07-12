using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Infrastructure.Time
{
    internal sealed class UtcClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
