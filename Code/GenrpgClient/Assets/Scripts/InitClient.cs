using UnityEngine;

using Genrpg.Shared.Utils;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Setup.Services;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using ClientEvents;
using System;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.Client.Updates;
using Assets.Scripts.Assets;
using Assets.Scripts.Awaitables;
using Genrpg.Shared.Core.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Core.Constants;


public class InitClient : BaseBehaviour, IInitClient
{

    [SerializeField]
    private ClientConfig _clientConfig;
    
    [SerializeField]
    private GameObject _splashImage;

    [SerializeField]
    private GameObject _globalRoot;

    public bool PlayerContainsAllAssets()
    {
        return _clientConfig.PlayerContainsAllAssets;
    }

    private IClientAuthService _loginService;
    private ICrawlerService _crawlerService;
    private IClientWebService _webService;
    private IClientConfigContainer _config;
    private IClientAppService _clientAppService;
    private IClientEntityService _entityService;
    private ICursorService _cursorService;
    private ILocalLoadService _localLoadService;
    private IAwaitableService _awaitableService;
    private IClientAuthService _clientAuthService;

#if UNITY_EDITOR
    public string CurrMapId;
    public static InitClient EditorInstance { get; set; }
    public int BlockCount;
    public float ZoneSize;
    public int ForceZoneTypeId;
    public int MapGenSeed;
    public float PlayerSpeedMult;
    public long AccountSuffixId;
#endif

    public EGameModes GameMode;
    private CancellationTokenSource _gameTokenSource = new CancellationTokenSource();

    private void Awake()
    {
        ReflectionUtils.AddAllowedAssembly(Assembly.GetExecutingAssembly());
    }

    public object GetRootObject()
    {
        return _globalRoot;
    }

    public void FullResetGame()
    {

        _awaitableService.ForgetAwaitable(FullResetGameAsync());
    }

    public void CleanupGame()
    {
        _awaitableService.ForgetAwaitable(CleanupGameAsync());
    }

    private async Awaitable CleanupGameAsync()
    {
        _clientEntityService.SetActive(_splashImage, true);
        _gameTokenSource?.Cancel();
        _gameTokenSource?.Dispose();
        _gameTokenSource = new CancellationTokenSource();
        _globalUpdater = null;

        List<IGameCleanup> cleanups = new List<IGameCleanup>();

        foreach (IInjectable inj in _gs.loc.GetVals())
        {
            if (inj is IGameCleanup cleanup)
            {
                await cleanup.OnCleanup(_gameTokenSource.Token);
            }
        }
        _clientEntityService.DestroyAllChildren(_globalRoot);
    }

    private async Awaitable FullResetGameAsync()
    {
        await CleanupGameAsync();
        await InitGameAsync();

    }

    async void Start()
    {
        await InitGameAsync();    
    }

    public IClientGameState InitialSetup()
    {

        _gs = new ClientGameState(_clientConfig);
        _gs.loc.Resolve(this);
        return _gs;
    }

    async Awaitable InitGameAsync()
    {
        InitialSetup();
#if UNITY_EDITOR
        EditorInstance = this;
#endif
        string envName = _config.Config.Env.ToString();

        _awaitableService.ForgetAwaitable(DelayRemoveSplashScreen(_gameTokenSource.Token));

        // Initial app appearance.
        _clientAppService.TargetFrameRate = 30;
        if (GameMode == EGameModes.MMO || GameMode == EGameModes.BoardGame)
        {
            _clientAppService.SetupScreen(2460, 1440, false, true, 2);
        }
        else
        {
           // _clientAppService.SetupScreen(3840, 2160, true, true, 0);
        }
        _dispatcher.AddListener<NewVersionEvent>(OnNewVersion, _gameTokenSource.Token);
        _gs.GameMode = GameMode;

        if (GameModeUtils.IsPureClientMode(_gs.GameMode))
        {
            await SetupGame(_gameTokenSource.Token);
            return;
        }

        ClientWebRequest req = new ClientWebRequest();
        string url = _config.Config.InitialConfigEndpoint + "?env=" + envName;

        _awaitableService.ForgetAwaitable(req.SendRequest(_logService, url, null, null, OnGetWebConfig, _gameTokenSource.Token));

        await Task.CompletedTask;
    }

    private void OnGetWebConfig(string txt, List<FullWebRequest> commands,  CancellationToken token)
    {
        _awaitableService.ForgetAwaitable(OnGetWebConfigAsync(SerializationUtils.Deserialize<ConfigResponse>(txt), token));
    }

    private async Awaitable OnGetWebConfigAsync(ConfigResponse response, CancellationToken token)
    {

        try
        {
            _gs.LoginServerURL = response.ServerURL;
            _config.Config.ResponseContentRoot = response.ContentRoot;
            _config.Config.ResponseAssetEnv = response.AssetEnv;
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

        InitialPrefabLoader prefabLoader = _localLoadService.LocalLoad<InitialPrefabLoader>("Prefabs/PrefabLoader");
        await prefabLoader.LoadPrefabs(_gs, _entityService, _localLoadService, _globalRoot);

        await clientInitializer.FinalInitialize(token);

        while (!_assetService.IsInitialized())
        {
            await Awaitable.WaitForSecondsAsync(0.001f);
        }

        _cursorService.SetCursor(CursorNames.Default);
        _screenService.Open(ScreenId.Loading);

        while (_screenService.GetScreen(ScreenId.Loading) == null)
        {
            await Awaitable.NextFrameAsync(cancellationToken: _gameTokenSource.Token);
        }

        _screenService.Open(ScreenId.FloatingText);

        if (!GameModeUtils.IsPureClientMode(_gs.GameMode))
        {
            _loginService.StartAuth(token);
        }
        else
        {
            await _loginService.StartNoUser(token);
        }
        string txt2 = "ScreenWH: " + _clientAppService.ScreenWidth + "x" + _clientAppService.ScreenHeight + " -- " + Game.Prefix + " -- " + _config.Config.Env + " -- " + _clientAppService.Platform;
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
            _clientEntityService.SetActive(_splashImage, false);
        }
    }

    private void OnNewVersion(NewVersionEvent newVersion)
    {
        Caching.ClearCache();
    }

    private IGlobalUpdater _globalUpdater;
    public void SetGlobalUpdater(IGlobalUpdater updater)
    {
        _globalUpdater = updater;
    }

    private void Update()
    {
        _globalUpdater?.OnUpdate();
    }

    private void LateUpdate()
    {
        _globalUpdater?.OnLateUpdate();
    }
}