using UnityEngine;
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
using Genrpg.Shared.Constants.TempDev;

public class InitClient : BaseBehaviour
{

    [SerializeField]
    private ClientConfig _config = null;

    [SerializeField]
    public GameObject _splashImage;

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
#endif

    public static InitClient Instance { get; set; }

    private CancellationTokenSource _gameTokenSource = new CancellationTokenSource();

    void Start()
    {
        UnityGameState gs = new UnityGameState();
        gs.logger = new ClientLogger(gs);
        gs.loc = new ServiceLocator(gs.logger);
        gs.repo = new ClientRepositorySystem(gs.logger);
        
        Initialize(gs);
        OnStart().Forget();
    }

    public void RemoveSplashScreen()
    {
        if (_splashImage != null)
        {
            GameObjectUtils.SetActive(_splashImage, false);
            _splashImage = null;
        }
    }

    private CancellationTokenSource _clientTokenSource = new CancellationTokenSource();

    private string _envName = "";
    private EnvEnum _envEnum = EnvEnum.Local;
    protected async UniTask OnStart()
    {
#if UNITY_EDITOR
        EditorInstance = this;
#endif
        Instance = this;
        _envName = _config.Env.ToString();
        _envEnum = _config.Env;

        // Initial app appearance.
        Application.targetFrameRate = 30;
        Screen.SetResolution(3840, 2160, FullScreenMode.FullScreenWindow);
        Analytics.Setup(_gs);
        QualitySettings.vSyncCount = 2;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Cursors.SetCursor(Cursors.Default);

        WebRequest req = new WebRequest();
        string url = ConfigURL + "?env=" + _envName;
        req.GetData(_gs, url, "", OnGetWebConfig, _gameTokenSource.Token).Forget();
        await UniTask.CompletedTask;
    }

    private void OnGetWebConfig(UnityGameState gs, string txt, CancellationToken token)
    {
        OnGetWebConfigAsync(gs, SerializationUtils.Deserialize<ConfigResponse>(txt), token).Forget();
    }

    private async UniTask OnGetWebConfigAsync(UnityGameState gs, ConfigResponse response, CancellationToken token)
    {
        _gs.SetInitObject(gameObject);
        _gs.Env = _envName;
        _gs.SiteURL = response.ServerURL;
        string envArtName = (_envName == EnvNames.Prod ? _envName.ToLower() : EnvNames.Dev.ToLower());
        _gs.ArtURL = response.ArtURLPrefix + Game.Prefix.ToLower() + envArtName + "/";
        UIHelper.SetGameState(_gs);

        // Basic game setup
        SetupService setupService = new SetupService();
        await setupService.SetupGame(_gs, token);

        ClientSetupService.SetupClient(_gs, true, token);

        InitialPrefabLoader prefabLoader = Resources.Load<InitialPrefabLoader>("Prefabs/PrefabLoader");
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

        _screenService.Open(_gs, ScreenId.Loading);

        while (_screenService.GetScreen(_gs, ScreenId.Loading) == null)
        {
            await UniTask.NextFrame(_gameTokenSource.Token);
        }

        _screenService.Open(_gs, ScreenId.FloatingText);

        _loginService.StartLogin(_gs, token);

        string txt2 = "ScreenWH: " + Screen.width + "x" + Screen.height + " -- " + Game.Prefix + " -- " + _envName + " -- " + Application.platform.ToString();
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