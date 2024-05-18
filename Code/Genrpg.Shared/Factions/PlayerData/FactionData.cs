using MessagePack;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Spells.Casting;
using Genrpg.Shared.Utils;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.Factions.Settings;
using Genrpg.Shared.Factions.Messages;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Units.Mappers;

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
    /// <summary>
    /// A list of affinities/reputations
    /// </summary>
    /// 
    [MessagePackObject]
    public class FactionData : OwnerIdObjectList<FactionStatus>
    {
        [Key(0)] public override string Id { get; set; }

        protected override void OnCreateChild(FactionStatus newChild)
        {
            newChild.RepLevelId = (newChild.IdKey == FactionTypes.Player ? RepLevels.Neutral : RepLevels.Hated);
        }

    }
    [MessagePackObject]
    public class FactionApi : OwnerApiList<FactionData, FactionStatus> { }
    [MessagePackObject]
    public class FactionDataLoader : OwnerIdDataLoader<FactionData, FactionStatus> { }


    [MessagePackObject]
    public class FactionDataMapper : OwnerDataMapper<FactionData, FactionStatus, FactionApi> { }
}
