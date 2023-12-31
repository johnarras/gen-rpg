using MessagePack;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genrpg.Shared.DataStores.PlayerData;

namespace Genrpg.Shared.Spells.PlayerData.Spells
{
    [MessagePackObject]
    public class SpellData : OwnerIdObjectList<Spell>
    {
        [Key(0)] public override string Id { get; set; }

        protected override bool CreateMissingChildOnGet() { return false; }

        public void Add(Spell spell)
        {
            _data = _data.Where(x => x.Id != spell.Id).ToList();
            _data.Add(spell);
        }

        public void Remove(long spellId)
        {
            _data = _data.Where(x => x.IdKey != spellId).ToList();
        }
    }
}
