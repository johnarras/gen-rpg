using MessagePack;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Stats.Entities;
using System.Collections.Generic;

namespace Genrpg.Shared.Inventory.Entities
{
    [MessagePackObject]
    public class SetPiece
    {
        [Key(0)] public long ItemTypeId { get; set; }
        [Key(1)] public string Name { get; set; }

        [Key(2)] public List<StatPct> Stats { get; set; }

        [Key(3)] public List<SpellProc> Procs { get; set; }

        public SetPiece()
        {
            Stats = new List<StatPct>();
            Procs = new List<SpellProc>();
        }

    }
}
