using MessagePack;
using Genrpg.Shared.Spells.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Genrpg.Shared.Inventory.PlayerData
{
    [MessagePackObject]
    public class ItemEffect : IEffect
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long Quantity { get; set; }
        [Key(2)] public long EntityId { get; set; }
    }

}
