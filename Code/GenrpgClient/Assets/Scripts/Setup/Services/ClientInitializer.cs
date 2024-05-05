using Assets.Scripts.Crawler.Services;
using Assets.Scripts.Crawler.Services.Combat;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Crawler.Services.Training;
using Assets.Scripts.Ftue.Services;
using Assets.Scripts.GameSettings.Services;
using Assets.Scripts.Model;
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ClientInitializer 
{
    public async Task SetupClientServices (UnityGameState gs, bool forRealGame, CancellationToken token)
    {
        gs.loc.Set<IAssetService>(new UnityAssetService());
        gs.loc.Set<IFileDownloadService>(new FileDownloadService());
        gs.loc.Set<IRepositoryService>(new ClientRepositoryService(gs.loc.Get<ILogService>()));
        gs.loc.Set<IShapeService>(new ShapeService());
        gs.loc.Set<INoiseService>(new NoiseService());
        gs.loc.Set<ISamplingService>(new SamplingService());
        gs.loc.Set<ILineGenService>(new LineGenService());
        gs.loc.Set<ILocationGenService>(new LocationGenService());
        gs.loc.Set<IMapGenService>(new MapGenService());
        gs.loc.Set<IZoneGenService>(new ZoneGenService());
        gs.loc.Set<IQuestGenService>(new QuestGenService());
        gs.loc.Set<IClientMapObjectManager>(new ClientMapObjectManager(token));
        gs.loc.Set<IClientGameDataService>(new ClientGameDataService());
        gs.loc.Set<IUIInitializable>(new UIInitializable());
        gs.loc.Set<IFxService>(new FxService());
        gs.loc.Set<IUnityUpdateService>(gs.AddComponent<UnityUpdateService>());

        // Unity-specific overrides

        if (forRealGame)
        {
            gs.loc.Set<IRealtimeNetworkService>(new RealtimeNetworkService(gs, token));
            gs.loc.Set<IWebNetworkService>(new WebNetworkService(gs, token));
            gs.loc.Set<IInputService>(gs.AddComponent<InputService>());
            gs.loc.Set<IMapTerrainManager>(gs.AddComponent<MapTerrainManager>());
            gs.loc.Set<ICrawlerService>(new CrawlerService());
            gs.loc.Set<ICrawlerSpellService>(new CrawlerSpellService());
            gs.loc.Set<ICombatService>(new CombatService());
            gs.loc.Set<ILootGenService>(new LootGenService());
            gs.loc.Set<IProcessCombatRoundCombatService>(new ProcessCombatRoundCombatService());
            gs.loc.Set<ITrainingService>(new TrainingService());
            gs.loc.Set<ICrawlerStatService>(new CrawlerStatService());
            gs.loc.Set<ICrawlerMapService>(new CrawlerMapService());
        }

        gs.loc.Set<IClientLoginService>(new ClientLoginService());
		gs.loc.Set<IUnitSetupService>(new UnitSetupService());
		gs.loc.Set<IMapGenService>(new MapGenService());
		gs.loc.Set<IZoneGenService> (new UnityZoneGenService());
        gs.loc.Set<IFtueService>(new ClientFtueService());

        gs.loc.ResolveSelf();

        List<IInjectable> vals = gs.loc.GetVals();
        foreach (IInjectable service in vals)
        {
            if (service is IInitializable initService)
            {
                await initService.Initialize(gs, token);
            }
        }
    }
}
