using Assets.Scripts.Crawler.Services;
using Assets.Scripts.Crawler.Services.Combat;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Crawler.Services.Training;
using Assets.Scripts.Ftue.Services;
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
    private IUnityGameState _gs = null;
    public ClientInitializer(IUnityGameState gs)
    {
        _gs = gs;
    }


    public void AddClientServices (InitClient initClient, bool forRealGame, CancellationToken token)
    {
        if (initClient != null)
        {
            _gs.loc.Set<IInitClient>(initClient);
        }
        _gs.loc.Set<IAssetService>(new UnityAssetService());
        _gs.loc.Set<IFileDownloadService>(new FileDownloadService());
        _gs.loc.Set<IRepositoryService>(new ClientRepositoryService(_gs.loc.Get<ILogService>()));
        _gs.loc.Set<IShapeService>(new ShapeService());
        _gs.loc.Set<INoiseService>(new NoiseService());
        _gs.loc.Set<ISamplingService>(new SamplingService());
        _gs.loc.Set<ILineGenService>(new LineGenService());
        _gs.loc.Set<ILocationGenService>(new LocationGenService());
        _gs.loc.Set<IMapGenService>(new MapGenService());
        _gs.loc.Set<IZoneGenService>(new ZoneGenService());
        _gs.loc.Set<IQuestGenService>(new QuestGenService());
        _gs.loc.Set<IClientMapObjectManager>(new ClientMapObjectManager(token));
        _gs.loc.Set<IClientGameDataService>(new ClientGameDataService());
        _gs.loc.Set<IUIService>(new UIInitializable());
        _gs.loc.Set<IFxService>(new FxService());
        _gs.loc.Set<IUnityUpdateService>(_gs.AddComponent<UnityUpdateService>());
        _gs.loc.Set<ITerrainPatchLoader>(new TerrainPatchLoader());
        _gs.loc.Set<IPlantAssetLoader>(new PlantAssetLoader());
        _gs.loc.Set<IZonePlantValidator>(new ZonePlantValidator());
        _gs.loc.Set<IAddPoolService>(new AddPoolService());
        _gs.loc.Set<ITerrainTextureManager>(new TerrainTextureManager());
        _gs.loc.Set<IPlayerManager>(new  PlayerManager());

        // Unity-specific overrides

        if (forRealGame)
        {
            _gs.loc.Set<IRealtimeNetworkService>(new RealtimeNetworkService(token));
            _gs.loc.Set<IWebNetworkService>(new WebNetworkService(token));
            _gs.loc.Set<IInputService>(_gs.AddComponent<InputService>());
            _gs.loc.Set<IMapTerrainManager>(_gs.AddComponent<MapTerrainManager>());
            _gs.loc.Set<ICrawlerService>(new CrawlerService());
            _gs.loc.Set<ICrawlerSpellService>(new CrawlerSpellService());
            _gs.loc.Set<ICombatService>(new CombatService());
            _gs.loc.Set<ILootGenService>(new LootGenService());
            _gs.loc.Set<IProcessCombatRoundCombatService>(new ProcessCombatRoundCombatService());
            _gs.loc.Set<ITrainingService>(new TrainingService());
            _gs.loc.Set<ICrawlerStatService>(new CrawlerStatService());
            _gs.loc.Set<ICrawlerMapService>(new CrawlerMapService());
            _gs.loc.Set<IClientPathfindingUtils>(new ClientPathfindingUtils());
            _gs.loc.Set<IAddRoadService>(new AddRoadService());  
        }

        _gs.loc.Set<IClientLoginService>(new ClientLoginService());
		_gs.loc.Set<IUnitSetupService>(new UnitSetupService());
		_gs.loc.Set<IMapGenService>(new MapGenService());
		_gs.loc.Set<IZoneGenService> (new UnityZoneGenService());
        _gs.loc.Set<IFtueService>(new ClientFtueService());

    }

    public async Task FinalInitialize(CancellationToken token)
    {
        _gs.loc.ResolveSelf();

        List<IInjectable> vals = _gs.loc.GetVals();
        foreach (IInjectable service in vals)
        {
            if (service is IInitializable initService)
            {
                await initService.Initialize(_gs, token);
            }
        }
        List<Task> setupTasks = new List<Task>();

        foreach (IInjectable service in _gs.loc.GetVals())
        {
            if (service is IInitializable initService)
            {
                setupTasks.Add(initService.Initialize(_gs, token));
            }
        }

        await Task.WhenAll(setupTasks);

        foreach (IInjectable service in _gs.loc.GetVals())
        {
            if (service is IGameTokenService gameTokenService)
            {
                gameTokenService.SetGameToken(token);
            }
        }
    }
}
