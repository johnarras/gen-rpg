using GEntity = UnityEngine.GameObject;
using Cysharp.Threading.Tasks;
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
        OnStart().Forget();
    }

    protected async UniTask OnStart()
    {
        UnityGameState gs = new UnityGameState();
        Initialize(gs);

#if UNITY_EDITOR
        EditorInstance = this;
#endif
        string envName = _gs.Config.Env.ToString();

        DelayRemoveSplashScreen(_gameTokenSource.Token).Forget();

        // Initial app appearance.
        AppUtils.TargetFrameRate = 30;
        ScreenUtils.SetupScreenSystem(1920, 1080, false, true, 2);
        Cursors.SetCursor(Cursors.Default);

        ClientWebRequest req = new ClientWebRequest();
        string url = _gs.Config.InitialConfigEndpoint + "?env=" + envName;
        req.SendRequest(_gs, _logService, url, "", OnGetWebConfig, _gameTokenSource.Token).Forget();
        await UniTask.CompletedTask;
    }

    private void OnGetWebConfig(UnityGameState gs, string txt, CancellationToken token)
    {
        OnGetWebConfigAsync(gs, SerializationUtils.Deserialize<ConfigResponse>(txt), token).Forget();
    }

    private async UniTask OnGetWebConfigAsync(UnityGameState gs, ConfigResponse response, CancellationToken token)
    {
        _gs.SetInitObject(entity);
        _dispatcher.AddEvent<NewVersionEvent>(this, OnNewVersion);
        _gs.LoginServerURL = response.ServerURL;
        _gs.Config.ResponseContentRoot = response.ContentRoot;
        _gs.Config.ResponseAssetEnv = response.AssetEnv;

        // Basic game setup
        SetupService setupService = new SetupService();
        await setupService.SetupGame(_gs, token);

        ClientInitializer clientInitializer = new ClientInitializer();
        clientInitializer.AddClientServices(_gs, this, true, token);

        InitialPrefabLoader prefabLoader = AssetUtils.LoadResource<InitialPrefabLoader>("Prefabs/PrefabLoader");
        await prefabLoader.LoadPrefabs(_gs);

        await clientInitializer.FinalInitialize(gs, token);

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

    private void OnNewVersion(NewVersionEvent newVersion)
    {
        Caching.ClearCache();
    }
}