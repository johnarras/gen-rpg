using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.GameSettings;

namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class LocationType : ChildSettings, IIndexedGameItem
    {
        public const int ZoneCenter = 1;
        public const int Secondary = 3;

        public const int MinSize = 4;

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }

        [Key(6)] public int XSize { get; set; }
        [Key(7)] public int YSize { get; set; }

        [Key(8)] public string SetupType { get; set; }
        [Key(9)] public string Art { get; set; }
    }
}
