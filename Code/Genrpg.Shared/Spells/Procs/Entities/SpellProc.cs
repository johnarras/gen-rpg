using Genrpg.Shared.Spells.Procs.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Procs.Entities
{
    [MessagePackObject]
    public class SpellProc : IProc
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public long MinQuantity { get; set; }
        [Key(3)] public long MaxQuantity { get; set; }
        [Key(4)] public double PercentChance { get; set; } = 100;
        [Key(5)] public long MaxCharges { get; set; }
        [Key(6)] public long CurrCharges { get; set; }
        [Key(7)] public long CooldownSeconds { get; set; }
        [Key(8)] public DateTime LastUsedTime { get; set; } = DateTime.MinValue;
        [Key(9)] public long ElementTypeId { get; set; }
        [Key(10)] public string Name { get; set; }
    }
}
