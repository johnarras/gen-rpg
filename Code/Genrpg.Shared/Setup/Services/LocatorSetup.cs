
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Currencies.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Crafting.Services;
using Genrpg.Shared.Stats.Services;
using Genrpg.Shared.Factions.Services;
using Genrpg.Shared.Quests.Services;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.UserCoins.Services;
using Genrpg.Shared.SpellCrafting.Services;
using Genrpg.Shared.Units.Services;
using Genrpg.Shared.Names.Services;
using Genrpg.Shared.Charms.Services;
using Genrpg.Shared.Ftue.Services;
using Genrpg.Shared.UnitEffects.Services;
using Genrpg.Shared.Crawler.Roles.Services;

namespace Genrpg.Shared.Setup.Services
{
    public class LocatorSetup : IFactorySetupService
    {
        public virtual void Setup(GameState gs)
        {
            IServiceLocator fact = gs.loc;
            fact.Set<IEntityService>(new EntityService());
            fact.Set<ISharedCraftingService>(new SharedCraftingService());
            fact.Set<ISharedSpellCraftService>(new SharedSpellCraftService());
            fact.Set<IStatService>(new SharedStatService());
            fact.Set<ISharedFactionService>(new SharedFactionService());
            fact.Set<INameGenService>(new NameGenService());
            fact.Set<IUnitGenService>(new UnitGenService());
            fact.Set<ISharedQuestService>(new SharedQuestService());
            fact.Set<IPathfindingService>(new PathfindingService());
            fact.Set<IInventoryService>(new InventoryService());
            fact.Set<ICurrencyService>(new CurrencyService());
            fact.Set<IUserCoinService>(new UserCoinService());
            fact.Set<ICharmService>(new CharmService());
            fact.Set<IFtueService>(new FtueService());
            fact.Set<IStatusEffectService>(new StatusEffectService());








            // This section is for dungeon crawler stuff.
            fact.Set<IClassService>(new ClassService());
           
        }
    }
}
