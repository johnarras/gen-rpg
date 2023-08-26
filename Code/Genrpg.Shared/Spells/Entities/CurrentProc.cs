using MessagePack;
using System;

namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class CurrentProc
    {
        [Key(0)] public long SpellTypeId { get; set; }
        [Key(1)] public DateTime CooldownEnds { get; set; }
    }
}
