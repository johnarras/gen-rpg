using MessagePack;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Spells.Messages;

namespace Genrpg.Shared.Spells.Casting
{
    [MessagePackObject]
    public class SpellHitData
    {

        [Key(0)] public int Id { get; set; }

        [Key(1)] public bool UpdatedStatEffect { get; set; }

        [Key(2)] public Unit OrigTarget { get; set; }

        [Key(3)] public Unit Target { get; set; }

        [Key(4)] public bool PrimaryTarget { get; set; }

        [Key(5)] public bool IsCrit { get; set; }
        [Key(6)] public long BaseAmount { get; set; }
        [Key(7)] public long DefenseAmount { get; set; }
        [Key(8)] public long MaxAmount { get; set; }
        [Key(9)] public long FinalAmount { get; set; }
        [Key(10)] public long Amount { get; set; }
        [Key(11)] public long AbsorbAmount { get; set; }
        [Key(12)] public int PowerPct { get; set; }
        [Key(13)] public SendSpell SendSpell { get; set; }

        public SpellHitData()
        {
        }
    }
}
