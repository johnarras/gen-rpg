using MessagePack;
using Genrpg.Shared.DataStores.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class SpellData : IdObjectList<Spell>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<Spell> Data { get; set; } = new List<Spell>();
        public override void AddTo(Unit unit) { unit.Set(this); }
        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }
        protected override bool CreateIfMissingOnGet()
        {
            return false;
        }

    }
}
