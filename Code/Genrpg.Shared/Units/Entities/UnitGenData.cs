using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils.Data;

namespace Genrpg.Shared.Units.Entities
{
    [MessagePackObject]
    public class UnitGenData
    {
        [Key(0)] public long UnitTypeId { get; set; }
        [Key(1)] public long Level { get; set; }
        [Key(2)] public long FactionTypeId { get; set; }
        [Key(3)] public long ZoneId { get; set; }


        // Immediately create a unit and use it to store UnitTypeId, Id and Level.
        [Key(4)] public Unit Unit { get; set; }
        [Key(5)] public MyPointF Pos { get; set; }
        [Key(6)] public short Rot { get; set; }
        [Key(7)] public object Parent { get; set; }
        [Key(8)] public int StatPct { get; set; }
        [Key(9)] public bool AllowNoParent { get; set; }

        public GameStateObjectDelegate Handler;
        public object ArtInstance;

        public UnitGenData()
        {
            StatPct = 100;
        }

    }
}
