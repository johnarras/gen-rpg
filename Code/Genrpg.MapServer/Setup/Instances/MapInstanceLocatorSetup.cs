using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Currencies.Services;
using Genrpg.Shared.Setup.Services;
using Genrpg.Shared.Entities.Services;
using Genrpg.ServerShared.GameSettings;
using Genrpg.MapServer.Vendors;
using Genrpg.MapServer.AI.Services;
using Genrpg.MapServer.Crafting;
using Genrpg.MapServer.Items;
using Genrpg.MapServer.Spawns;
using Genrpg.MapServer.Quests;
using Genrpg.MapServer.Entities;
using Genrpg.MapServer.Currencies;
using Genrpg.MapServer.Stats;
using Genrpg.MapServer.Maps;
using Genrpg.ServerShared.Setup;
using Genrpg.MapServer.Units;
using Genrpg.MapServer.Spells;
using Genrpg.MapServer.Levelup;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.UserCoins.Services;
using Genrpg.MapServer.UserCoins;

namespace Genrpg.MapServer.Setup
{
    public class MapInstanceLocatorSetup : BaseServerLocatorSetup
    {
        public override void Setup(GameState gs)
        {
            IServiceLocator fact = gs.loc;
            IRepositorySystem repo = gs.repo;

            base.Setup(gs);

            fact.Set<IServerSpellService>(new ServerSpellService());
            fact.Set<IServerUnitService>(new ServerUnitService());
            fact.Set<IVendorService>(new VendorService());
            fact.Set<IAIService>(new AIService());
            fact.Set<IServerCraftingService>(new ServerCraftingService());
            fact.Set<IItemService>(new ItemService());
            fact.Set<ISpawnService>(new SpawnService());
            fact.Set<IItemGenService>(new ItemGenService());
            fact.Set<ILevelService>(new LevelService());
            fact.Set<IMapMessageService>(new MapMessageService());
            fact.Set<IServerQuestService>(new ServerQuestService());
            fact.Set<IEntityService>(new ServerEntityService());
            fact.Set<IInventoryService>(new ServerInventoryService());
            fact.Set<ICurrencyService>(new ServerCurrencyService());
            fact.Set<IStatService>(new ServerStatService());
            fact.Set<IMapObjectManager>(new MapObjectManager());
            fact.Set<IUserCoinService>(new ServerUserCoinService());
        }
    }
}
