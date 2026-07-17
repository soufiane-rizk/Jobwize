using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.ModuleOne
{
    public interface INotificationStore
    {
        List<Guid> Published { get; }
    }
    internal class NotificationStore : INotificationStore
    {
        public List<Guid> Published { get; } = new List<Guid>();
    }
}
