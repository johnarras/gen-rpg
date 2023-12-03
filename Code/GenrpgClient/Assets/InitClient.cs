using GEntity = UnityEngine.GameObject;
using System.Threading.Tasks;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Setup.Services;
using UI.Screens.Constants;
using System.Threading;
using Genrpg.Shared.Interfaces;
using Assets.Scripts.Tokens;
using Genrpg.Shared.Core.Entities;
using Assets.Scripts.Model;
using Genrpg.Shared.Constants.TempDev;
using UI.Screens.Utils;
using Genrpg.Shared.GameSettings;

public class InitClient : BaseBehaviour
{

    public ClientConfig _config = null;
    public GEntity _splashImage;

    private IClientLoginService _loginService;

    const string ConfigURL = TempDevConstants.ConfigEndpoint;

    public const string DefaultAssetPrefix = TempDevConstants.DefaultAssetPrefix;

    public string CurrMapId;

#if UNITY_EDITOR

    public static InitClient EditorInstance { get; set; }
    public bool ForceDownloadFromAssetBundles = false;
    public int CurrWorldSize;
    public float CurrZoneSize;
    public int MapGenSeed;
    public float PlayerSpeedMult;
    public int ZoneNoiseSize = 4096;
    public float ZoneNoiseDenominator = 36;
    public float ZoneNoiseAmplitude = 1.0f;
    public float ZoneNoisePersistence = 0.3f;
    public float ZoneNoiseLacunarity = 2.0f;
#endif

    public static InitClient Instance { get; set; }

    private CancellationTokenSource _gameTokenSource = new CancellationTokenSource();

    void Start()
    {
        UnityGameState gs = new UnityGameState();
        gs.logger = new ClientLogger(gs);
        gs.loc = new ServiceLocator(gs.logger);
        gs.repo = new ClientRepositorySystem(gs.logger);
        gs.data = new GameData();

        Initialize(gs);

        TaskUtils.AddTask(OnStart(),"onstart", _gameTokenSource.Token);

    }

    public void RemoveSplashScreen()
    {
        if (_splashImage != null)
        {
            GEntityUtils.SetActive(_splashImage, false);
            _splashImage = null;
        }
    }


    private CancellationTokenSource _clientTokenSource = new CancellationTokenSource();

    private string _envName = "";
    private EnvEnum _envEnum = EnvEnum.Local;
    protected async Task OnStart()
    {
#if UNITY_EDITOR
        EditorInstance = this;
#endif
        Instance = this;
        _envName = _config.Env.ToString();
        _envEnum = _config.Env;

        Analytics.Setup(_gs);
        // Initial app appearance.
        AppUtils.TargetFrameRate = 30;
        ScreenUtils.SetupScreenSystem(3840, 2160, true, true, 2);
        Cursors.SetCursor(Cursors.Default);

        ClientWebRequest req = new ClientWebRequest();
        string url = ConfigURL + "?env=" + _envName;
        TaskUtils.AddTask(req.SendRequest(_gs, url, "", OnGetWebConfig, _gameTokenSource.Token),"getwebconfig",_gameTokenSource.Token);
        await Task.CompletedTask;
    }

    private void OnGetWebConfig(UnityGameState gs, string txt, CancellationToken token)
    {
        TaskUtils.AddTask(OnGetWebConfigAsync(gs, SerializationUtils.Deserialize<ConfigResponse>(txt), token),"ongetwebconfigasync", _gameTokenSource.Token);
    }

    private async Task OnGetWebConfigAsync(UnityGameState gs, ConfigResponse response, CancellationToken token)
    {
        _gs.SetInitObject(entity);
        _gs.Env = _envName;
        _gs.SiteURL = response.ServerURL;
        string envArtName = (_envName == EnvNames.Prod ? _envName.ToLower() : EnvNames.Dev.ToLower());
        _gs.ArtURL = response.ArtURLPrefix + Game.Prefix.ToLower() + envArtName + "/";
        UIHelper.SetGameState(_gs);

        // Basic game setup
        SetupService setupService = new SetupService();
        await setupService.SetupGame(_gs, token);

        ClientSetupService.SetupClient(_gs, true, token);

        InitialPrefabLoader prefabLoader = AssetUtils.LoadResource<InitialPrefabLoader>("Prefabs/PrefabLoader");
        await prefabLoader.LoadPrefabs(_gs);

        _gs.loc.ResolveSelf();
        _gs.loc.Resolve(this);

        foreach (IService service in _gs.loc.GetVals())
        {
            if (service is ISetupService iss)
            {
                await iss.Setup(_gs, _gameTokenSource.Token);
            }
        }
        foreach (IService service in _gs.loc.GetVals())
        {
            if (service is IGameTokenService gameTokenService)
            {
                gameTokenService.SetGameToken(_gameTokenSource.Token);
            }
        }

        while (!_assetService.IsInitialized(gs))
        {
            await Task.Delay(1);
        }

        _screenService.Open(_gs, ScreenId.Loading);

        while (_screenService.GetScreen(_gs, ScreenId.Loading) == null)
        {
            await Task.Delay(1, _gameTokenSource.Token);
        }

        _screenService.Open(_gs, ScreenId.FloatingText);

        _loginService.StartLogin(_gs, token);

        string txt2 = "ScreenWH: " + ScreenUtils.Width + "x" + ScreenUtils.Height + " -- " + Game.Prefix + " -- " + _envName + " -- " + AppUtils.Platform;
        _gs.logger.Info(txt2);
    }

    void OnApplicationQuit()
    {
        _gameTokenSource.Cancel();
        _networkService?.CloseClient();
    }

    public CancellationToken GetGameToken()
    {
        return _gameTokenSource.Token;
    }
}