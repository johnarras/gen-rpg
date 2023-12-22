using Genrpg.Shared.DataStores.PlayerData;
using MessagePack;

namespace Genrpg.Shared.Crafting.PlayerData.Crafting
{
    [MessagePackObject]
    public class CraftingData : OwnerIdObjectList<CraftingStatus>
    {
        [Key(0)] public override string Id { get; set; }
    }
}
