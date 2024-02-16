using Assets.Scripts.Crawler.Services;
using Assets.Scripts.Crawler.Services.Combat;
using Assets.Scripts.Crawler.Services.Training;
using Assets.Scripts.Ftue.Services;
using Assets.Scripts.GameSettings.Services;
using Assets.Scripts.UI.Services;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Ftue.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System.Threading;

public class ClientInitializer 
{
    public void SetupClientServices (UnityGameState gs, bool forRealGame, string assetPrefix,
        string contentDataEnv, string worldDataEnv, CancellationToken token)
    {
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
        gs.loc.Set<IUiService>(new UiService());
        gs.loc.Set<IFxService>(new FxService());

        // Unity-specific overrides
        IAssetService ias = new UnityAssetService();
        ias.Init(gs, assetPrefix, contentDataEnv, worldDataEnv, token);
        gs.loc.Set(ias);

        if (forRealGame)
        {
            gs.loc.Set<IRealtimeNetworkService>(new RealtimeNetworkService(gs, token));
            gs.loc.Set<IWebNetworkService>(new WebNetworkService(gs, token));
            gs.loc.Set<IUnityUpdateService>(gs.AddComponent<UnityUpdateService>());
            gs.loc.Set<IInputService>(gs.AddComponent<InputService>());
            gs.loc.Set<IMapTerrainManager>(gs.AddComponent<MapTerrainManager>());
        }

        gs.loc.Set<IClientLoginService>(new ClientLoginService());
		gs.loc.Set<IUnitSetupService>(new UnitSetupService());
		gs.loc.Set<IMapGenService>(new MapGenService());
		gs.loc.Set<IZoneGenService> (new UnityZoneGenService());
        gs.loc.Set<IFtueService>(new ClientFtueService());

        gs.loc.Set<ICrawlerService>(new CrawlerService());
        gs.loc.Set<ICrawlerSpellService>(new CrawlerSpellService());
        gs.loc.Set<ICombatService>(new CombatService());
        gs.loc.Set<ILootGenService>(new LootGenService());
        gs.loc.Set<IProcessCombatRoundCombatService>(new ProcessCombatRoundCombatService());
        gs.loc.Set<ITrainingService>(new TrainingService());
        gs.loc.Set<ICrawlerStatService>(new CrawlerStatService());

        gs.loc.ResolveSelf();

        foreach (IService service in gs.loc.GetVals())
        {
            if (service is ISetupService setupService)
            {
                setupService.Setup(gs, token);
            }
        }
    }
}
