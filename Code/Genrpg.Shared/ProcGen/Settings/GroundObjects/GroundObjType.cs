using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.GameSettings;

namespace Genrpg.Shared.ProcGen.Settings.GroundObjects
{


    [MessagePackObject]
    public class GroundObjType : ChildSettings, IIndexedGameItem
    {
        public const string ChestGroup = "chest";
        public const string MineralGroup = "mineral";
        public const string HerbGroup = "herb";
        public const string WoodGroup = "wood";

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public string GroupId { get; set; }
        [Key(8)] public int SpawnWeight { get; set; }
        [Key(9)] public long CrafterTypeId { get; set; }
        [Key(10)] public long SpawnTableId { get; set; }
        [Key(11)] public int MinRolls { get; set; }
        [Key(12)] public int MaxRolls { get; set; }
        [Key(13)] public long QualityTypeId { get; set; }
        [Key(14)] public bool OneTimeOnly { get; set; }


        public static int GetPositionHash(int x, int y)
        {
            return x / 16 + y / 16 << 10;
        }

    }
}
