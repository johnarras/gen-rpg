using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Settings.Spells
{
    [MessagePackObject]
    public class SpellType : ChildSettings, ISpell
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }

        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public long ElementTypeId { get; set; }
        [Key(8)] public long PowerStatTypeId { get; set; }
        [Key(9)] public int PowerCost { get; set; }
        [Key(10)] public int Cooldown { get; set; }
        [Key(11)] public float CastTime { get; set; }
        [Key(12)] public int Range { get; set; }
        [Key(13)] public int MaxCharges { get; set; }
        [Key(14)] public int Shots { get; set; }

        [Key(15)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        [Key(16)] public List<SpellEffect> Effects { get; set; } = new List<SpellEffect>();

        public void SetDirty(bool value) { }

        public SpellType()
        {
        }
    }
}
