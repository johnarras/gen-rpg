using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class SpellType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }

        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public string StringId { get; set; }

        [Key(8)] public long ElementTypeId { get; set; }
        [Key(9)] public long SkillTypeId { get; set; }

        [Key(10)] public int CastingTime { get; set; }
        [Key(11)] public int Cost { get; set; }
        [Key(12)] public int Cooldown { get; set; }
        [Key(13)] public int Range { get; set; }
        [Key(14)] public int Radius { get; set; }
        [Key(15)] public int Duration { get; set; }
        [Key(16)] public int Shots { get; set; }
        [Key(17)] public int MaxCharges { get; set; }
        [Key(18)] public int ExtraTargets { get; set; }
        [Key(19)] public int Scale { get; set; }
        [Key(20)] public int ComboGen { get; set; }

        [Key(21)] public int FinalScale { get; set; }

        [Key(22)] public long OrigSpellTypeId { get; set; }

        [Key(23)] public List<SpellProc> Procs { get; set; }

        [Key(24)] public DateTime CooldownEnds { get; set; }
        [Key(25)] public int CurrCharges { get; set; }

        [Key(26)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        public SpellType()
        {
            FinalScale = 100;
            Procs = new List<SpellProc>();
        }
    }
}
