using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Website.Interfaces;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Website.Messages.RefreshStores
{
    [MessagePackObject]
    public class RefreshStoresResult : IWebResult
    {
        [Key(0)] public PlayerStoreOfferData Stores { get; set; }
    }
}
