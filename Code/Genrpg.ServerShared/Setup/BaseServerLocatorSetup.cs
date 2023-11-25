using Genrpg.ServerShared.Accounts.Services;
using Genrpg.ServerShared.Achievements;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.Maps;
using Genrpg.ServerShared.MapSpawns;
using Genrpg.ServerShared.PlayerData;
using Genrpg.ServerShared.Purchasing.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Setup
{
    public class BaseServerLocatorSetup : LocatorSetup
    {
        public override void Setup(GameState gs)
        {
            base.Setup(gs);
            gs.loc.Set<ICloudCommsService>(new CloudCommsService());
            gs.loc.Set<IGameDataService>(new GameDataService<GameData>());
            gs.loc.Set<IMapSpawnService>(new MapSpawnService());
            gs.loc.Set<IMapDataService>(new MapDataService());
            gs.loc.Set<IAchievementService>(new AchievementService());
            gs.loc.Set<IPlayerDataService>(new PlayerDataService());
            gs.loc.Set<IPurchasingService>(new PurchasingService());
            gs.loc.Set<IAccountService>(new AccountService());
            gs.loc.Set<IAdminService>(new BaseAdminService());
        }
    }
}
