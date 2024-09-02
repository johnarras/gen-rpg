using GEntity = UnityEngine.GameObject;

using Genrpg.Shared.Utils;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Setup.Services;
using UI.Screens.Constants;
using System.Threading;
using Genrpg.Shared.Interfaces;
using UI.Screens.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using ClientEvents;
using UnityEngine;
using System;
using Assets.Scripts.Crawler.Services;
using static ClientWebService;


public interface IInitClient : IInjectable
{
    GameObject go { get; }
}
public class InitClient : BaseBehaviour, IInitClient
{
    public GEntity _splashImage;
    public GameObject go { get { return gameObject; } }


    private IClientAuthService _loginService;
    private ICrawlerService _crawlerService;
    private IClientWebService _webService;

#if UNITY_EDITOR
    public string CurrMapId;
    public static InitClient EditorInstance { get; set; }
    public bool ForceDownloadFromAssetBundles = false;
    public int BlockCount;
    public float ZoneSize;
    public int ForceZoneTypeId;
    public int MapGenSeed;
    public float PlayerSpeedMult;
    public long AccountSuffixId;
#endif
    public bool CrawlerMode = false;
    private CancellationTokenSource _gameTokenSource = new CancellationTokenSource();

    private void Awake()
    {
        ReflectionUtils.AddAllowedAssembly(Assembly.GetExecutingAssembly());
    }

    void Start()
    {
        AwaitableUtils.ForgetAwaitable(OnStart());
    }

    public IUnityGameState InitialSetup()
    {

        _gs = new UnityGameState();
        _gs.loc.Resolve(this);
        return _gs;
    }

    protected async Awaitable OnStart()
    {
        InitialSetup();
#if UNITY_EDITOR
        EditorInstance = this;
#endif
        string envName = base._gs.Config.Env.ToString();

        AwaitableUtils.ForgetAwaitable(DelayRemoveSplashScreen(_gameTokenSource.Token));

        // Initial app appearance.
        AppUtils.TargetFrameRate = 30;
        ScreenUtils.SetupScreenSystem(2460, 1440, false, true, 2);
        Cursors.SetCursor(Cursors.Default);

        _dispatcher.AddEvent<NewVersionEvent>(this, OnNewVersion);
        _gs.CrawlerMode = CrawlerMode;

        if (_gs.CrawlerMode)
        {
            await SetupGame(_gameTokenSource.Token);
            return;
        }

        ClientWebRequest req = new ClientWebRequest();
        string url = base._gs.Config.InitialConfigEndpoint + "?env=" + envName;

        AwaitableUtils.ForgetAwaitable(req.SendRequest(_logService, url, null, null, OnGetWebConfig, _gameTokenSource.Token));

        await Task.CompletedTask;
    }

    private void OnGetWebConfig(string txt, List<FullWebCommand> commands,  CancellationToken token)
    {
        AwaitableUtils.ForgetAwaitable(OnGetWebConfigAsync(SerializationUtils.Deserialize<ConfigResponse>(txt), token));
    }

    private async Awaitable OnGetWebConfigAsync(ConfigResponse response, CancellationToken token)
    {

        try
        {
            _gs.LoginServerURL = response.ServerURL;
            _gs.Config.ResponseContentRoot = response.ContentRoot;
            _gs.Config.ResponseAssetEnv = response.AssetEnv;
            await SetupGame(token);
        }
        catch (Exception e)
        {
            _logService.Exception(e, "AfterConfig");
        }
    }

    private async Awaitable SetupGame(CancellationToken token)
    {

        // Basic game setup
        SetupService setupService = new SetupService(_gs.loc);
        await setupService.SetupGame(token);

        ClientInitializer clientInitializer = new ClientInitializer(_gs);
        clientInitializer.AddClientServices(this, true, token);

        InitialPrefabLoader prefabLoader = AssetUtils.LoadResource<InitialPrefabLoader>("Prefabs/PrefabLoader");
        await prefabLoader.LoadPrefabs(_gs);

        await clientInitializer.FinalInitialize(token);

        while (!_assetService.IsInitialized())
        {
            await Awaitable.WaitForSecondsAsync(0.001f);
        }

        _screenService.Open(ScreenId.Loading);

        while (_screenService.GetScreen(ScreenId.Loading) == null)
        {
            await Awaitable.NextFrameAsync(cancellationToken: _gameTokenSource.Token);
        }

        _screenService.Open(ScreenId.FloatingText);

        if (!_gs.CrawlerMode)
        {
            _loginService.StartAuth(token);
        }
        else
        {
            await _loginService.StartNoUser(token);
        }
        string txt2 = "ScreenWH: " + ScreenUtils.Width + "x" + ScreenUtils.Height + " -- " + Game.Prefix + " -- " + _gs.Config.Env + " -- " + AppUtils.Platform;
        _logService.Info(txt2);
    }
    

    void OnApplicationQuit()
    {
        _gameTokenSource.Cancel();
        _gameTokenSource.Dispose();
        _gameTokenSource = null;
        _networkService?.CloseClient();
        _crawlerService?.SaveGame();
    }

    public CancellationToken GetGameToken()
    {
        return _gameTokenSource.Token;
    }

    private async Awaitable DelayRemoveSplashScreen(CancellationToken token)
    {
        while (_screenService == null || _screenService.GetAllScreens().Count < 1)
        {
            await Awaitable.NextFrameAsync(token);
        }

        if (_splashImage != null)
        {
            GEntityUtils.SetActive(_splashImage, false);
            _splashImage = null;
        }
    }

    private void OnNewVersion(NewVersionEvent newVersion)
    {
        Caching.ClearCache();
    }
}