﻿using Genrpg.ServerShared.Accounts.Services;
using Genrpg.ServerShared.Achievements;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Crypto.Services;
using Genrpg.ServerShared.DataStores;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.Maps;
using Genrpg.ServerShared.MapSpawns;
using Genrpg.ServerShared.PlayerData;
using Genrpg.ServerShared.Purchasing.Services;
using Genrpg.Shared.DataStores.Entities;
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
    public class BaseServerSetupService : SetupService
    {
        public BaseServerSetupService(IServiceLocator loc) : base(loc) { }

        protected override void AddServices()
        {
            base.AddServices();
            Set<IRepositoryService>(new ServerRepositoryService());
            Set<ICloudCommsService>(new CloudCommsService());
            Set<IGameDataService>(new GameDataService<GameData>());
            Set<IMapSpawnDataService>(new MapSpawnDataService());
            Set<IMapDataService>(new MapDataService());
            Set<IAchievementService>(new AchievementService());
            Set<IPlayerDataService>(new PlayerDataService());
            Set<IPurchasingService>(new PurchasingService());
            Set<IAccountService>(new AccountService());
            Set<IAdminService>(new BaseAdminService());
            Set<ICryptoService>(new CryptoService());
        }
    }
}