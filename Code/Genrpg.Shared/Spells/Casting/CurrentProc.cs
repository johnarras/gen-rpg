using MessagePack;
using System;

namespace Genrpg.Shared.Spells.Casting
{
    [MessagePackObject]
    public class CurrentProc
    {
        [Key(0)] public long SpellTypeId { get; set; }
        [Key(1)] public DateTime CooldownEnds { get; set; }
    }
}
