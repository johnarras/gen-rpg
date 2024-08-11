using MessagePack;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Factions.Constants;

namespace Genrpg.Shared.Factions.PlayerData
{
    [MessagePackObject]
    public class ReputationStatus : OwnerQuantityChild
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public override long IdKey { get; set; }
        [Key(3)] public long RepLevelId { get; set; }
        [Key(4)] public override long Quantity { get; set; }
    }
    /// <summary>
    /// A list of affinities/reputations
    /// </summary>
    /// 
    [MessagePackObject]
    public class ReputationData : OwnerQuantityObjectList<ReputationStatus>
    {
        [Key(0)] public override string Id { get; set; }

        protected override void OnCreateChild(ReputationStatus newChild)
        {
            newChild.RepLevelId = newChild.IdKey == FactionTypes.Player ? RepLevels.Neutral : RepLevels.Hated;
        }

    }
    [MessagePackObject]
    public class ReputationApi : OwnerApiList<ReputationData, ReputationStatus> { }
    [MessagePackObject]
    public class ReputationDataLoader : OwnerIdDataLoader<ReputationData, ReputationStatus> { }


    [MessagePackObject]
    public class ReputationDataMapper : OwnerDataMapper<ReputationData, ReputationStatus, ReputationApi> { }
}
