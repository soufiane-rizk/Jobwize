using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Domain
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; }
    }
}
