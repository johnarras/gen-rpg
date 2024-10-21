using Genrpg.Shared.BoardGame.Services;
using Genrpg.Shared.Charms.Services;
using Genrpg.Shared.Crafting.Services;
using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Crawler.Tavern.Services;
using Genrpg.Shared.Crawler.Temples.Services;
using Genrpg.Shared.Crawler.TimeOfDay.Services;
using Genrpg.Shared.Crawler.Training.Services;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Factions.Services;
using Genrpg.Shared.Ftue.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Names.Services;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.PlayMultiplier.Services;
using Genrpg.Shared.ProcGen.Services;
using Genrpg.Shared.Quests.Services;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.SpellCrafting.Services;
using Genrpg.Shared.Spells.Services;
using Genrpg.Shared.Stats.Services;
using Genrpg.Shared.Tasks.Services;
using Genrpg.Shared.UnitEffects.Services;
using Genrpg.Shared.Units.Services;
using Genrpg.Shared.UserAbilities.Services;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Setup.Services
{
    public class SetupService : IInitializable
    {

        protected IServiceLocator _loc;

        public SetupService(IServiceLocator loc)
        {
            _loc = loc;
        }

        protected void Set<T>(T t) where T : IInjectable
        {
            _loc.Set(t);
        }

        public async Task Initialize( CancellationToken toke)
        {
            await Task.CompletedTask;
        }

        protected virtual void AddServices()
        {
            Set<IEntityService>(new EntityService());
            Set<ISharedCraftingService>(new SharedCraftingService());
            Set<ISharedSpellCraftService>(new SharedSpellCraftService());
            Set<IStatService>(new SharedStatService());
            Set<ISharedFactionService>(new SharedFactionService());
            Set<INameGenService>(new NameGenService());
            Set<IUnitGenService>(new UnitGenService());
            Set<ISharedQuestService>(new SharedQuestService());
            Set<IPathfindingService>(new PathfindingService());
            Set<ICombatAbilityService>(new CombatAbilityService());
            Set<IInventoryService>(new InventoryService());
            Set<ICharmService>(new CharmService());
            Set<IFtueService>(new FtueService());
            Set<IStatusEffectService>(new StatusEffectService());
            Set<IMapProvider>(new MapProvider());
            Set<IItemGenService>(new ItemGenService());
            Set<IRewardService>(new RewardService());
            Set<IUserAbilityService>(new UserAbilityService());
            Set<ISharedPlayMultService>(new SharedPlayMultService());
            Set<ILineGenService>(new LineGenService());
            Set<INoiseService>(new NoiseService());
            Set<ISamplingService>(new SamplingService());
            Set<ICrawlerStatService>(new CrawlerStatService());
            Set<ITrainingService>(new TrainingService());  
            Set<ILootGenService>(new LootGenService()); 
            Set<ITimeOfDayService>(new TimeOfDayService()); 
            Set<ICrawlerCombatService>(new CrawlerCombatService());
            Set<IProcessCombatRoundCombatService>(new ProcessCombatRoundCombatService());   
            Set<ICrawlerSpellService>(new CrawlerSpellService()); 
            Set<ISharedItemService>(new SharedItemService());
            Set<ITaskService>(new TaskService());   
            Set<ITavernService>(new TavernService());   
            Set<ITempleService>(new TempleService());   
            // Use for crawler

            // Board game
            Set<ISharedBoardGenService>(new SharedBoardGenService());   
        }

        public virtual async Task SetupGame(CancellationToken token)
        {
            AddServices();
            _loc.ResolveSelf();
            _loc.Resolve(this);
            await ReflectionUtils.InitializeServiceList(_loc, _loc.GetVals(), token);

        }

        public virtual async Task FinalSetup()
        {
            await Task.CompletedTask;
        }

        public virtual bool CreateMissingGameData()
        {
            return false;
        }
    }
}
