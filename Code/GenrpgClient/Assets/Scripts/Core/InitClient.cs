using GEntity = UnityEngine.GameObject;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Setup.Services;
using UI.Screens.Constants;
using System.Threading;
using Genrpg.Shared.Interfaces;
using Assets.Scripts.Tokens;
using Genrpg.Shared.Core.Entities;
using Assets.Scripts.Model;
using UI.Screens.Utils;
using Genrpg.Shared.GameSettings;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;

public class InitClient : BaseBehaviour
{

    private ClientConfig _config = null;

    public GEntity _splashImage;

    private IClientLoginService _loginService;

#if UNITY_EDITOR
    public string CurrMapId;
    public static InitClient EditorInstance { get; set; }
    public bool ForceDownloadFromAssetBundles = false;
    public int WorldSize;
    public float ZoneSize;
    public int ForceZoneTypeId;
    public int MapGenSeed;
    public float PlayerSpeedMult;
#endif


    public const bool ForceCrawler = true;

    private CancellationTokenSource _gameTokenSource = new CancellationTokenSource();

    private void Awake()
    {
        ReflectionUtils.AddAllowedAssembly(Assembly.GetExecutingAssembly());
    }

    void Start()
    {
        OnStart().Forget();
    }

    private async UniTask DelayRemoveSplashScreen(CancellationToken token)
    {
        while (_screenService == null || _screenService.GetAllScreens(_gs).Count < 1)
        {
            await UniTask.NextFrame(token);
        }

        if (_splashImage != null)
        {
            GEntityUtils.SetActive(_splashImage, false);
            _splashImage = null;
        }
    }


    private string _envName = "";
    protected async UniTask OnStart()
    {
        _config = ClientConfig.Load();

        UnityGameState gs = new UnityGameState();
        gs.logger = new ClientLogger(gs);
        gs.loc = new ServiceLocator(gs.logger);
        gs.repo = new ClientRepositorySystem(gs.logger);
        gs.data = new GameData();

        Initialize(gs);

#if UNITY_EDITOR
        EditorInstance = this;
#endif
        _envName = _config.Env.ToString();

        DelayRemoveSplashScreen(_gameTokenSource.Token).Forget();

        Analytics.Setup(_gs);
        // Initial app appearance.
        AppUtils.TargetFrameRate = 30;
        ScreenUtils.SetupScreenSystem(1920, 1080, false, true, 2);
        Cursors.SetCursor(Cursors.Default);

        ClientWebRequest req = new ClientWebRequest();
        string url = _config.InitialConfigEndpoint + "?env=" + _envName;
        req.SendRequest(_gs, url, "", OnGetWebConfig, _gameTokenSource.Token).Forget();
        await UniTask.CompletedTask;
    }

    private void OnGetWebConfig(UnityGameState gs, string txt, CancellationToken token)
    {
        OnGetWebConfigAsync(gs, SerializationUtils.Deserialize<ConfigResponse>(txt), token).Forget();
    }

    private async UniTask OnGetWebConfigAsync(UnityGameState gs, ConfigResponse response, CancellationToken token)
    {
        _gs.SetInitObject(entity);
        _gs.Env = _envName;
        _gs.SiteURL = response.ServerURL;

        string artURLWithoutEnv = response.ArtURLPrefix;
        string contentDataEnv = string.IsNullOrEmpty(_config.ContentDataEnvOverride) ? response.ArtEnv : _config.ContentDataEnvOverride;
        string worldDataEnv = _config.WorldDataEnv;

        // Basic game setup
        SetupService setupService = new SetupService();
        await setupService.SetupGame(_gs, token);

        ClientInitializer clientInitilizer = new ClientInitializer();
        clientInitilizer.SetupClientServices(_gs, true, artURLWithoutEnv, contentDataEnv, worldDataEnv, token);

        InitialPrefabLoader prefabLoader = AssetUtils.LoadResource<InitialPrefabLoader>("Prefabs/PrefabLoader");
        await prefabLoader.LoadPrefabs(_gs);

        _gs.loc.ResolveSelf();
        _gs.loc.Resolve(this);

        List<Task> setupTasks = new List<Task>();

        foreach (IService service in _gs.loc.GetVals())
        {
            if (service is ISetupService iss)
            {
                setupTasks.Add(iss.Setup(_gs, _gameTokenSource.Token));
            }
        }

        await Task.WhenAll(setupTasks);

        foreach (IService service in _gs.loc.GetVals())
        {
            if (service is IGameTokenService gameTokenService)
            {
                gameTokenService.SetGameToken(_gameTokenSource.Token);
            }
        }

        while (!_assetService.IsInitialized(gs))
        {
            await UniTask.Delay(1);
        }

        _screenService.Open(_gs, ScreenId.Loading);

        while (_screenService.GetScreen(_gs, ScreenId.Loading) == null)
        {
            await UniTask.NextFrame( cancellationToken: _gameTokenSource.Token);
        }

        _screenService.Open(_gs, ScreenId.FloatingText);

        if (!ForceCrawler)
        {
            _loginService.StartLogin(_gs, token);
        }
        else
        {
            _loginService.NoUserGetGameData(token);
        }
        string txt2 = "ScreenWH: " + ScreenUtils.Width + "x" + ScreenUtils.Height + " -- " + Game.Prefix + " -- " + _envName + " -- " + AppUtils.Platform;
        _gs.logger.Info(txt2);
    }

    void OnApplicationQuit()
    {
        _gameTokenSource.Cancel();
        _gameTokenSource.Dispose();
        _gameTokenSource = null;
        _networkService?.CloseClient();
    }

    public CancellationToken GetGameToken()
    {
        return _gameTokenSource.Token;
    }
}