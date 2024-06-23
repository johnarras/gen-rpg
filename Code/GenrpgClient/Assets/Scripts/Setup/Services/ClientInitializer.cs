using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.Services;
using Assets.Scripts.Crawler.Services.Combat;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Crawler.Services.Training;
using Assets.Scripts.Ftue.Services;
using Assets.Scripts.GameObjects;
using Assets.Scripts.GameSettings.Services;
using Assets.Scripts.Model;
using Assets.Scripts.Pathfinding.Utils;
using Assets.Scripts.ProcGen.Loading.Utils;
using Assets.Scripts.Tokens;
using Assets.Scripts.UI.Services;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Ftue.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ClientInitializer 
{
    private IServiceLocator _loc = null;
    private IUnityGameState _gs;
    public ClientInitializer(IUnityGameState gs)
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

        Set<IGameObjectService>(new GameObjectService());
        _loc.ResolveSelf();

        IGameObjectService gameObjectService = _loc.Get<IGameObjectService>();
       
        Set<IAssetService>(new UnityAssetService());
        Set<IFileDownloadService>(new FileDownloadService());
        Set<IRepositoryService>(new ClientRepositoryService(_loc.Get<ILogService>()));
        Set<IShapeService>(new ShapeService());
        Set<INoiseService>(new NoiseService());
        Set<ISamplingService>(new SamplingService());
        Set<ILineGenService>(new LineGenService());
        Set<ILocationGenService>(new LocationGenService());
        Set<IMapGenService>(new MapGenService());
        Set<IZoneGenService>(new ZoneGenService());
        Set<IQuestGenService>(new QuestGenService()); 
        Set<ITerrainPatchLoader>(new TerrainPatchLoader());
        Set<IClientMapObjectManager>(new ClientMapObjectManager(token));
        Set<IClientGameDataService>(new ClientGameDataService());
        Set<IUIService>(new UIInitializable());
        Set<IFxService>(new FxService());
        Set<IUnityUpdateService>(gameObjectService.GetOrAddComponent<UnityUpdateService>());
        Set<IPlantAssetLoader>(new PlantAssetLoader());
        Set<IZonePlantValidator>(new ZonePlantValidator());
        Set<IAddPoolService>(new AddPoolService());
        Set<ITerrainTextureManager>(new TerrainTextureManager());
        Set<IPlayerManager>(new  PlayerManager());
        Set<IAddNearbyItemsHelper>(new AddNearbyItemsHelper());

        // Unity-specific overrides

        if (forRealGame)
        {
            Set<IRealtimeNetworkService>(new RealtimeNetworkService(token));
            Set<IWebNetworkService>(new WebNetworkService(token));
            Set<IInputService>(gameObjectService.GetOrAddComponent<InputService>());
            Set<IMapTerrainManager>(gameObjectService.GetOrAddComponent<MapTerrainManager>());
            Set<ICrawlerService>(new CrawlerService());
            Set<ICrawlerSpellService>(new CrawlerSpellService());
            Set<ICombatService>(new CombatService());
            Set<ILootGenService>(new LootGenService());
            Set<IProcessCombatRoundCombatService>(new ProcessCombatRoundCombatService());
            Set<ITrainingService>(new TrainingService());
            Set<ICrawlerStatService>(new CrawlerStatService());
            Set<ICrawlerMapService>(new CrawlerMapService());
            Set<ICrawlerWorldService>(new CrawlerWorldService());
            Set<IClientPathfindingUtils>(new ClientPathfindingUtils());
            Set<IAddRoadService>(new AddRoadService());  
        }

        Set<IClientLoginService>(new ClientLoginService());
		Set<IUnitSetupService>(new UnitSetupService());
		Set<IMapGenService>(new MapGenService());
		Set<IZoneGenService> (new UnityZoneGenService());
        Set<IFtueService>(new ClientFtueService());

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
