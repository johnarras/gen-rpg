using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Units.Loaders;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.PlayerData
{
    public class CurrentStoresLoader : UnitDataLoader<PlayerStoreOfferData>
    {
        public override bool SendToClient() { return false; }
        protected override bool IsUserData() { return true; }
    }


    [MessagePackObject]
    public class PlayerStoreOfferData : BasePlayerData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public DateTime GameDataSaveTime { get; set; } = DateTime.UtcNow;

        [Key(2)] public DateTime LastTimeSet { get; set; } = DateTime.MinValue;

        [Key(3)] public List<PlayerStoreOffer> StoreOffers { get; set; } = new List<PlayerStoreOffer>();
    }
}
