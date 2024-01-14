using MessagePack;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Spells.Casting;

namespace Genrpg.Shared.Inventory.Settings.ItemSets
{
    [MessagePackObject]
    public class SetType : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string NameId { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }

        [Key(8)] public List<SetPiece> Pieces { get; set; }

        [Key(9)] public List<SetStat> Stats { get; set; }

        [Key(10)] public List<SetSpellProc> Procs { get; set; }


        public SetType()
        {
            Pieces = new List<SetPiece>();
            Stats = new List<SetStat>();
            Procs = new List<SetSpellProc>();
        }
    }
    [MessagePackObject]
    public class SetStat : IStatPct
    {
        [Key(0)] public int ItemCount { get; set; }
        [Key(1)] public long StatTypeId { get; set; }
        [Key(2)] public int Percent { get; set; }
    }
    [MessagePackObject]
    public class SetSpellProc : ISpellProc
    {
        [Key(0)] public int Chance { get; set; }
        [Key(1)] public long SpellId { get; set; }
        [Key(2)] public int Cooldown { get; set; }
        [Key(3)] public long ProcTypeId { get; set; }
        [Key(4)] public long FromElementTypeId { get; set; }
        [Key(5)] public long FromSkillTypeId { get; set; }
        [Key(6)] public int Scale { get; set; }
        [Key(7)] public int ItemCount { get; set; }
    }
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
