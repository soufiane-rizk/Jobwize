using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.ModuleTwo
{
    public interface IModuleTwoNotificationStore
    {
        List<Guid> ItemCreatedReceived { get; }

        List<Guid> ItemIndexedReceived { get; }
    }

    internal class ModuleTwoNotificationStore : IModuleTwoNotificationStore
    {
        public List<Guid> ItemCreatedReceived { get; } = new List<Guid>();

        public List<Guid> ItemIndexedReceived { get; } = new List<Guid>();
    }
}