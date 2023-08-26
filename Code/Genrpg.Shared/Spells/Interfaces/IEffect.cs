using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Interfaces
{
    public interface IEffect
    {
        public long EntityTypeId { get; set; }

        public long Quantity { get; set; }

        public long EntityId { get; set; }
    }
}
