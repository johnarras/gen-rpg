using Assets.Scripts.BoardGame.Controllers;
using Assets.Scripts.BoardGame.Services;
using Assets.Scripts.Crawler.Maps;
using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.Services;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Ftue.Services;
using Assets.Scripts.GameObjects;
using Assets.Scripts.GameSettings.Services;
using Assets.Scripts.Lockouts.Services;
using Assets.Scripts.Pathfinding.Utils;
using Assets.Scripts.PlayerSearch;
using Assets.Scripts.ProcGen.Loading.Utils;
using Assets.Scripts.Rewards.Services;
using Assets.Scripts.TextureLists.Services;
using Assets.Scripts.UI.Services;
using Genrpg.Shared.Client.Assets.Services;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Client.Tokens;
using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.MapGen.Services;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.Crawler.Roguelikes.Services;
using Genrpg.Shared.Crawler.Roles.Services;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Crawler.Tavern.Services;
using Genrpg.Shared.Crawler.Temples.Services;
using Genrpg.Shared.Crawler.TextureLists.Services;
using Genrpg.Shared.Crawler.TimeOfDay.Services;
using Genrpg.Shared.Crawler.Training.Services;
using Genrpg.Shared.Ftue.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.UI.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class ClientSetupService 
{
    private IServiceLocator _loc = null;
    private IClientGameState _gs = null;
    public ClientSetupService(IClientGameState gs)
    {
        _gs = gs;
        _loc = gs.loc;
    }

    private void Set<T>(T obj) where T : IInjectable
    {
        _loc.Set(obj);
    }

    public void AddClientServices (InitClient initClient, bool forRealGame, CancellationToken token)
    {
        _loc.Set(_gs); 
        if (initClient != null)
        {
            Set<IInitClient>(initClient);
        }

        Set<IClientEntityService>(new ClientEntityService());
        _loc.ResolveSelf();

        IClientEntityService gameObjectService = _loc.Get<IClientEntityService>();
        
        Set<ISingletonContainer>(new SingletonContainer()); 
        Set<IAssetService>(new UnityAssetService());
        Set<IFileDownloadService>(new FileDownloadService());
        Set<IShapeService>(new ShapeService());
        Set<ILocationGenService>(new LocationGenService());
        Set<IMapGenService>(new MapGenService());
        Set<IZoneGenService>(new ZoneGenService());
        Set<IQuestGenService>(new QuestGenService()); 
        Set<ITerrainPatchLoader>(new TerrainPatchLoader());
        Set<IClientMapObjectManager>(new ClientMapObjectManager());
        Set<IClientGameDataService>(new ClientGameDataService());
        Set<IUIService>(new UIService());
        Set<IFxService>(new FxService());
        Set<ILockoutManager>(new LockoutManager());
        Set<IClientUpdateService>(new ClientUpdateService());
        Set<IPlantAssetLoader>(new PlantAssetLoader());
        Set<IZonePlantValidator>(new ZonePlantValidator());
        Set<IAddPoolService>(new AddPoolService());
        Set<ITerrainTextureManager>(new TerrainTextureManager());
        Set<IPlayerManager>(new  PlayerManager());
        Set<IAddNearbyItemsHelper>(new AddNearbyItemsHelper());
        Set<IPlayerSearchService>(new PlayerSearchService());
        Set<ITextService>(new TextService());
        Set<IIconService>(new IconService());   
        Set<IClientCryptoService>(new ClientCryptoService());
        Set<IBinaryFileRepository>(new BinaryFileRepository()); 
        Set<ICursorService>(new CursorService());
        Set<IModTextureService>(new ModTextureService());
        Set<IRewardService>(new ClientRewardService());
        Set<ITextureListCache>(new TextureListCache());

        // Unity-specific overrides

        if (forRealGame)
        {
            Set<IRealtimeNetworkService>(new RealtimeNetworkService());
            Set<IClientWebService>(new ClientWebService());
            Set<IMapTerrainManager>(new MapTerrainManager());
            Set<IInputService>(new InputService());
            Set<ICrawlerService>(new CrawlerService());
            Set<ICrawlerMapService>(new CrawlerMapService());
            Set<ICrawlerWorldService>(new CrawlerWorldService());
            Set<ICrawlerMapGenService>(new CrawlerMapGenService());
            Set<IClientPathfindingUtils>(new ClientPathfindingUtils());
            Set<IAddRoadService>(new AddRoadService());  
            Set<IShowDiceRollService>(new ShowDiceRollService());   
            Set<IBoardPrizeService>(new BoardPrizeService());   
        }

        Set<IClientAuthService>(new ClientAuthService());
		Set<IUnitSetupService>(new UnitSetupService());
		Set<IMapGenService>(new MapGenService());
		Set<IZoneGenService> (new UnityZoneGenService());
        Set<IFtueService>(new ClientFtueService());


        Set<ICrawlerStatService>(new CrawlerStatService());
        Set<ITrainingService>(new TrainingService());
        Set<ILootGenService>(new LootGenService());
        Set<ITimeOfDayService>(new TimeOfDayService());
        Set<ICrawlerCombatService>(new CrawlerCombatService());
        Set<IProcessCombatRoundCombatService>(new ProcessCombatRoundCombatService());
        Set<ICrawlerSpellService>(new CrawlerSpellService());
        Set<ITavernService>(new TavernService());
        Set<ITempleService>(new TempleService());
        Set<IRoleService>(new RoleService());
        Set<IRoguelikeUpgradeService>(new RoguelikeUpgradeService());
        Set<IInfoService>(new InfoService());
        Set<IPartyService>(new PartyService());

        Set<IBoardGameController>(new  BoardGameController());  
        Set<ILoadBoardService>(new LoadBoardService());
    }

    public async Task FinalInitialize(CancellationToken token)
    {
        _loc.ResolveSelf();

        List<IInjectable> vals = _loc.GetVals();

        foreach (IInjectable service in vals)
        {
            if (service is IInitializable initService)
            {
                await initService.Initialize(token);
            }
        }
        List<Task> setupTasks = new List<Task>();

        foreach (IInjectable service in _loc.GetVals())
        {
            if (service is IInitializable initService)
            {
                setupTasks.Add(initService.Initialize(token));
            }
        }

        await Task.WhenAll(setupTasks);

        foreach (IInjectable service in _loc.GetVals())
        {
            if (service is IGameTokenService gameTokenService)
            {
                gameTokenService.SetGameToken(token);
            }
        }
    }
}
