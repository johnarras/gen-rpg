using Genrpg.Shared.Spells.Procs.Interfaces;
using MessagePack;
using System;

namespace Genrpg.Shared.Spells.Casting
{
    public interface IOldSpellProc
    {

        int Chance { get; set; }
        long SpellId { get; set; }
        int Cooldown { get; set; }
        long ProcTypeId { get; set; }
        long FromElementTypeId { get; set; }
        long FromSkillTypeId { get; set; }
        int Scale { get; set; }
    }

    [MessagePackObject]
    public class OldSpellProc : IOldSpellProc
    {
        [Key(0)] public int Chance { get; set; }
        [Key(1)] public long SpellId { get; set; }
        [Key(2)] public int Cooldown { get; set; }
        [Key(3)] public long ProcTypeId { get; set; }
        [Key(4)] public long FromElementTypeId { get; set; }
        [Key(5)] public long FromSkillTypeId { get; set; }
        [Key(6)] public int Scale { get; set; }



        public static OldSpellProc CreateFrom(IOldSpellProc iproc)
        {
            return new OldSpellProc()
            {
                Chance = iproc.Chance,
                SpellId = iproc.SpellId,
                Cooldown = iproc.Cooldown,
                ProcTypeId = iproc.ProcTypeId,
                FromElementTypeId = iproc.FromElementTypeId,
                FromSkillTypeId = iproc.FromSkillTypeId,
                Scale = iproc.Scale
            };
        }
    }
}
