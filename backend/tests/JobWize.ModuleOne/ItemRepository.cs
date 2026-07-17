using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.ModuleOne
{
    public interface IItemRepository
    {
        Guid Create(string name);
    }
    internal sealed class ItemRepository : IItemRepository
    {
        public static readonly Guid CreatedId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public Guid Create(string name)
        {
            return CreatedId;
        }
    }
}
