using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.Categories.GameSettings;

namespace Genrpg.Shared.ProcGen.Settings.Locations
{
    [MessagePackObject]
    public class LocationType : ChildSettings, IIndexedGameItem
    {

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
