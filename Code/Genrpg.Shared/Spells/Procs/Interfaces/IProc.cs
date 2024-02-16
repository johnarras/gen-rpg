using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Procs.Interfaces
{
    public interface IProc
    {
        long EntityTypeId { get; set; }
        long EntityId { get; set; }
        long ElementTypeId { get; set; }
        long MinQuantity { get; set; }
        long MaxQuantity { get; set; }
        double PercentChance { get; set; }
    }
}
