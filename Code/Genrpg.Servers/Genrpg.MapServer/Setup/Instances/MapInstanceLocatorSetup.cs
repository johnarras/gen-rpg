using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Currencies.Services;
using Genrpg.Shared.Setup.Services;
using Genrpg.Shared.Entities.Services;
using Genrpg.ServerShared.GameSettings;
using Genrpg.MapServer.AI.Services;
using Genrpg.MapServer.Items;
using Genrpg.MapServer.Entities;
using Genrpg.MapServer.Currencies;
using Genrpg.MapServer.Stats;
using Genrpg.MapServer.Maps;
using Genrpg.ServerShared.Setup;
using Genrpg.MapServer.Units;
using Genrpg.MapServer.Spells;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.MapServer.MapMessaging;
using Genrpg.MapServer.UserCoins;
using Genrpg.Shared.UserCoins.Services;
using Genrpg.MapServer.Trades.Services;
using Genrpg.MapServer.Crafting.Services;
using Genrpg.MapServer.Items.Services;
using Genrpg.MapServer.Levelup.Services;
using Genrpg.MapServer.Quests.Services;
using Genrpg.MapServer.Spawns.Services;
using Genrpg.MapServer.Spells.Services;
using Genrpg.MapServer.Units.Services;
using Genrpg.MapServer.Vendors.Services;
using Genrpg.MapServer.MapMessaging.Services;
using Genrpg.MapServer.Entities.Services;
using Genrpg.MapServer.Currencies.Services;
using Genrpg.MapServer.Stats.Services;
using Genrpg.MapServer.UserCoins.Services;

namespace Genrpg.MapServer.Setup.Instances
{
    public class MapInstanceLocatorSetup : BaseServerLocatorSetup
    {
        public override void Setup(IGameState gs)
        {
            IServiceLocator fact = gs.loc;
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
            fact.Set<ITradeService>(new TradeService());
        }
    }
}
