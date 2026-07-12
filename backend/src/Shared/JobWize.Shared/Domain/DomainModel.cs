using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Domain
{
    public abstract class DomainModel : Entity
    {
        public DateTime CreatedAt { get; private set; }

        public DateTime? UpdatedAt { get; private set; }

        public DateTime? DeletedAt { get; private set; }
    }
}
