using MessagePack;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Inventory.Entities
{
    [MessagePackObject]
    public class SetType : IIndexedGameItem
    {
        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string NameId { get; set; }
        [Key(3)] public string Desc { get; set; }
        [Key(4)] public string Icon { get; set; }
        [Key(5)] public string Art { get; set; }

        [Key(6)] public List<SetPiece> Pieces { get; set; }

        [Key(7)] public List<SetStat> Stats { get; set; }

        [Key(8)] public List<SetSpellProc> Procs { get; set; }


        public SetType()
        {
            Pieces = new List<SetPiece>();
            Stats = new List<SetStat>();
            Procs = new List<SetSpellProc>();
        }
    }
}
