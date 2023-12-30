using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils.Data;
using MessagePack;

namespace Genrpg.Shared.MapMods.Settings
{
    [MessagePackObject]
    public class MapModEffectType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }

        [Key(6)] public string Art { get; set; }
    }
}
