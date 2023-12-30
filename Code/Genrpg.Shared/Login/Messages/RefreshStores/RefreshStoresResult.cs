using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Purchasing.PlayerData;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Login.Messages.RefreshStores
{
    [MessagePackObject]
    public class RefreshStoresResult : ILoginResult
    {
        [Key(0)] public PlayerStoreOfferData Stores { get; set; }
    }
}
