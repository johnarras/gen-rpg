using MessagePack;
using Genrpg.Shared.Spells.Casting;

namespace Genrpg.Shared.Inventory.Settings.ItemSets
{
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
}
