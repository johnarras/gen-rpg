using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Interfaces
{
    public interface ISpell : IIndexedGameItem
    {
        long IdKey { get; set; }
        string Name { get; set; }
        string Desc { get; set; }
        string Icon { get; set; }
        string Art { get; set; }
        long PowerStatTypeId { get; set; }
        long ElementTypeId { get; set; }
        int PowerCost { get; set; }
        int Cooldown { get; set; }
        float CastTime { get; set; }
        int MaxCharges { get; set; }
        int Range { get; set; }
        int Shots { get; set; }
        
        public List<SpellEffect> Effects { get; set; }

        void SetDirty(bool isDirty);
    }
}
