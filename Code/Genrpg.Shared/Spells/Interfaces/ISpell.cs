using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Settings.Spells;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Interfaces
{
    public interface ISpell : IIndexedGameItem
    {
        long PowerStatTypeId { get; set; }
        long ElementTypeId { get; set; }
        int PowerCost { get; set; }
        int Cooldown { get; set; }
        float CastTime { get; set; }
        int MaxCharges { get; set; }
        int MinRange { get; set; }
        int MaxRange { get; set; }
        int Shots { get; set; }

        public List<SpellEffect> Effects { get; set; }

        void SetDirty(bool isDirty);
    }
}
