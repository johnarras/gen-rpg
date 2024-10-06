using MessagePack;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;

namespace Genrpg.Shared.BoardGame.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class MarkerData : OwnerQuantityObjectList<MarkerStatus>
    {
        [Key(0)] public override string Id { get; set; }

        public long GetQuantity(long MarkerTypeId)
        {
            return Get(MarkerTypeId).Quantity;
        }
    }

    [MessagePackObject]
    public class MarkerStatus : OwnerQuantityChild
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public override long IdKey { get; set; }
        [Key(3)] public override long Quantity { get; set; }

    }

    [MessagePackObject]
    public class MarkerApi : OwnerApiList<MarkerData, MarkerStatus> { }

    [MessagePackObject]
    public class MarkerDataLoader : OwnerIdDataLoader<MarkerData, MarkerStatus> { }



    [MessagePackObject]
    public class MarkerDataMapper : OwnerDataMapper<MarkerData, MarkerStatus, MarkerApi> { }
}
