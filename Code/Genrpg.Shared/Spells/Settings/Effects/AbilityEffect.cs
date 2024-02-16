using Genrpg.Shared.Spells.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Settings.Effects
{ 

    /// <summary>
    /// These are used for passive bonuses that skills and elements give players.
    /// </summary>
    [MessagePackObject]
    public class AbilityEffect : IEffect
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long Quantity { get; set; }
        [Key(2)] public long EntityId { get; set; }
    }
}
