using MessagePack;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Units.Loaders;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Quests.PlayerData;
using Genrpg.Shared.Units.Mappers;

namespace Genrpg.Shared.Purchasing.PlayerData
{

    public class CurrentPurchaseLoader : UnitDataLoader<CurrentPurchaseData>
    {
    }

    [MessagePackObject]
    public class CurrentPurchaseData : NoChildPlayerData, IUserData
    {

        [Key(0)] public override string Id { get; set; }

        [Key(1)] public PlayerStoreOffer CurrentOffer { get; set; }

        [Key(2)] public PlayerOfferProduct CurrentProduct { get; set; }
    }

    [MessagePackObject]
    public class CurrentPurchaseMapper : UnitDataMapper<CurrentPurchaseData> 
    {
        public override bool SendToClient() { return false; }
    }
}
