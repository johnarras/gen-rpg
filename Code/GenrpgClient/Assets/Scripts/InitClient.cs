using GEntity = UnityEngine.GameObject;

using Genrpg.Shared.Utils;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Setup.Services;
using UI.Screens.Constants;
using System.Threading;
using Genrpg.Shared.Interfaces;
using Assets.Scripts.Tokens;
using UI.Screens.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using Genrpg.Shared.Analytics.Services;
using ClientEvents;
using Genrpg.Shared.Core.Entities;
using UnityEngine;
using System.Runtime.InteropServices;


public interface IInitClient : IInjectable
{

}
public class InitClient : BaseBehaviour, IInitClient
{
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

    public const bool ForceCrawler = false;

    private CancellationTokenSource _gameTokenSource = new CancellationTokenSource();

    private void Awake()
    {
        ReflectionUtils.AddAllowedAssembly(Assembly.GetExecutingAssembly());
    }

    void Start()
    {
        OnStart();
    }

    protected async Awaitable OnStart()
    {
        IUnityGameState gs = new UnityGameState();
        Initialize(gs);

#if UNITY_EDITOR
        EditorInstance = this;
#endif
        string envName = base._gs.Config.Env.ToString();

        DelayRemoveSplashScreen(_gameTokenSource.Token);

        // Initial app appearance.
        AppUtils.TargetFrameRate = 30;
        ScreenUtils.SetupScreenSystem(1920, 1080, false, true, 2);
        Cursors.SetCursor(Cursors.Default);

        ClientWebRequest req = new ClientWebRequest();
        string url = base._gs.Config.InitialConfigEndpoint + "?env=" + envName;
        req.SendRequest(_logService, url, "", OnGetWebConfig, _gameTokenSource.Token);
        
    }

    private void OnGetWebConfig(string txt, CancellationToken token)
    {
        OnGetWebConfigAsync(SerializationUtils.Deserialize<ConfigResponse>(txt), token);
    }

    private async Awaitable OnGetWebConfigAsync(ConfigResponse response, CancellationToken token)
    {
        _gs.SetInitObject(entity);
        _dispatcher.AddEvent<NewVersionEvent>(this, OnNewVersion);
        _gs.LoginServerURL = response.ServerURL;
        _gs.Config.ResponseContentRoot = response.ContentRoot;
        _gs.Config.ResponseAssetEnv = response.AssetEnv;

        // Basic game setup
        SetupService setupService = new SetupService();
        await setupService.SetupGame(_gs, token);

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

        if (!ForceCrawler)
        {
            _loginService.StartLogin(token);
        }
        else
        {
            _loginService.NoUserGetGameData(token);
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