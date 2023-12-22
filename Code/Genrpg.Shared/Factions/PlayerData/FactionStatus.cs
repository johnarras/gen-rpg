using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.Categories.PlayerData;

namespace Genrpg.Shared.Factions.PlayerData
{
    [MessagePackObject]
    public class FactionStatus : OwnerPlayerData, IId
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public long RepLevelId { get; set; }
        [Key(4)] public long Reputation { get; set; }
    }
}
