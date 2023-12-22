using Genrpg.Shared.DataStores.PlayerData;
using MessagePack;

namespace Genrpg.Shared.Charms.PlayerData
{
    [MessagePackObject]
    public class PlayerCharmData : OwnerIdObjectList<PlayerCharm>
    {
        [Key(0)] public override string Id { get; set; }
    }
}
