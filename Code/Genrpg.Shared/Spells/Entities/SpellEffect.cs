using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class SpellEffect
    {
        [Key(0)] public long SkillTypeId { get; set; } = 1;
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public int Radius { get; set; }
        [Key(3)] public int Duration { get; set; }
        [Key(4)] public int ExtraTargets { get; set; }
        [Key(5)] public int Scale { get; set; }
        [Key(6)] public int Flags { get; set; }

        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }
    }
}
