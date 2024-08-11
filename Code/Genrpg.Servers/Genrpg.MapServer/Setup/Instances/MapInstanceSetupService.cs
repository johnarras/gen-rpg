using Genrpg.MapServer.AI.Services;
using Genrpg.MapServer.Crafting.Services;
using Genrpg.MapServer.Items.Services;
using Genrpg.MapServer.Levelup.Services;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.MapServer.MapMessaging.Services;
using Genrpg.MapServer.Maps;
using Genrpg.MapServer.Quests.Services;
using Genrpg.MapServer.Rewards.Services;
using Genrpg.MapServer.Spawns.Services;
using Genrpg.MapServer.Spells.Services;
using Genrpg.MapServer.Stats.Services;
using Genrpg.MapServer.Trades.Services;
using Genrpg.MapServer.Units.Services;
using Genrpg.MapServer.Vendors.Services;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.Maps;
using Genrpg.ServerShared.MapSpawns;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Setup.Services;
using Genrpg.Shared.Spawns.Entities;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Setup.Instances
{
    public class MapInstanceSetupService : BaseServerSetupService
    {
        public MapInstanceSetupService(IServiceLocator loc) : base(loc) { }

        protected override void AddServices()
        {
            base.AddServices();
            Set<IServerSpellService>(new ServerSpellService());
            Set<IServerUnitService>(new ServerUnitService());
            Set<IVendorService>(new VendorService());
            Set<IAIService>(new AIService());
            Set<IServerCraftingService>(new ServerCraftingService());
            Set<IItemService>(new ItemService());
            Set<ISpawnService>(new SpawnService());
            Set<ILevelService>(new LevelService());
            Set<IMapMessageService>(new MapMessageService());
            Set<IServerQuestService>(new ServerQuestService());
            Set<IRewardService>(new ServerRewardService());
            Set<IInventoryService>(new ServerInventoryService());
            Set<IStatService>(new ServerStatService());
            Set<IMapObjectManager>(new MapObjectManager());
            Set<ITradeService>(new TradeService());
        }

        public override async Task FinalSetup()
        {
            await base.FinalSetup();
        }
    }
}
