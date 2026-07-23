using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.ModuleOne
{
    public interface IModuleOneNotificationStore
    {
        List<Guid> Published { get; }
    }
    internal class ModuleOneNotificationStore : IModuleOneNotificationStore
    {
        public List<Guid> Published { get; } = new List<Guid>();
    }
}
