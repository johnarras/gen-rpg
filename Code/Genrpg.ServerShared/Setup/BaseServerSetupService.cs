using Genrpg.ServerShared.Accounts.Services;
using Genrpg.ServerShared.Achievements;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Crypto.Services;
using Genrpg.ServerShared.DataStores;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.Maps;
using Genrpg.ServerShared.MapSpawns;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Setup.Services;
using Genrpg.ServerShared.Secrets.Services;

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
            Set<IAccountService>(new AccountService());
            Set<IAdminService>(new BaseAdminService());
            Set<ICryptoService>(new CryptoService());
            Set<ISecretsService>(new SecretsService()); 
        }
    }
}
