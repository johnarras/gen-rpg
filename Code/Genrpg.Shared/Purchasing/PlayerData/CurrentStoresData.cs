using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Units.Loaders;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.PlayerData
{
    public class CurrentStoresLoader : UnitDataLoader<CurrentStoresData>
    {
        public override bool SendToClient() { return false; }
    }


    [MessagePackObject]
    public class CurrentStoresData : BasePlayerData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public DateTime LastGameDataSaveTime { get; set; }

        [Key(2)] public DateTime LastTimeSet { get; set; } = DateTime.MinValue;

        [Key(3)] public List<CurrentStore> Stores { get; set; } = new List<CurrentStore>();
    }


    [MessagePackObject]
    public class CurrentStore
    {
        [Key(0)] public long StoreOfferTypeId { get; set; }
        [Key(1)] public string UniqueId { get; set; }
    }
}
