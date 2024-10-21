using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Characters.PlayerData;
using System.Collections.Generic;
using Genrpg.Shared.MapServer.Entities;

using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.Interfaces;
using Assets.Scripts.GameSettings.Entities;
using Genrpg.Shared.DataStores.Entities;
using Assets.Scripts.Model;
using Genrpg.Shared.Client.Core;
using Assets.Scripts.Assets;
using Assets.Scripts.Awaitables;

public class ClientGameState : GameState, IInjectable, IClientGameState
{
    public IMapGenData md { get; set; } = null;   
    public User user { get; set; }
    public Character ch { get; set; }
    public bool CrawlerMode { get; set; } = false;
    public List<CharacterStub> characterStubs { get; set; }  = new List<CharacterStub>();
    public List<MapStub> mapStubs { get; set; } = new List<MapStub>(); 


    public string LoginServerURL { get; set; }
    public string Version { get; set; }
    public string RealtimeHost { get; set; }
    public string RealtimePort { get; set; }

    private ILogService _logService;
    private IClientAppService _clientAppService;
    protected IAwaitableService _awaitableService;
    public ClientGameState(ClientConfig config)
    {
        ILocalLoadService localLoadService = new LocalLoadService();
        ClientConfigContainer configContainer = new ClientConfigContainer();
        configContainer.Config = config;
        _logService = new ClientLogger(configContainer.Config);

       
        IAnalyticsService analyticsService = new ClientAnalyticsService(configContainer.Config);
        loc = new ServiceLocator(_logService, analyticsService, new ClientGameData());
        loc.Set<IClientConfigContainer>(configContainer);
        loc.Set<IAwaitableService>(new AwaitableService());
        loc.Set<IDispatcher>(new Dispatcher());
        loc.Set<IClientGameState>(this);
        loc.Set<IClientRandom>(new ClientRandom());
        loc.Set<IMapGenData>(new MapGenData());
        loc.Set<IClientAppService>(new ClientAppService());
        loc.Set<ILocalLoadService>(localLoadService);
        loc.Set<IRepositoryService>(new ClientRepositoryService(_logService));  
    }

    protected string ConfigFilename = "InitialClientConfig";
    protected InitialClientConfig _config = null;
    public InitialClientConfig GetConfig()
    {
        if (_config == null)
        {
            ClientRepositoryCollection<InitialClientConfig> repo = new ClientRepositoryCollection<InitialClientConfig>(_logService, _clientAppService);
            _config = repo.Load(ConfigFilename).GetAwaiter().GetResult();
            if (_config == null)
            {
                _config = new InitialClientConfig()
                {
                    Id = ConfigFilename,
                };
                // Do this here rather than in constructor because protobuf will ignore zeroes
                _config.UserFlags |= UserFlags.SoundActive | UserFlags.MusicActive;
                SaveConfig();
            }
        }
        return _config;
    }

    public void SaveConfig()
    {
        if (_config == null)
        {
            _config = new InitialClientConfig()
            {
                Id = ConfigFilename,
            };
        }

        ClientRepositoryCollection<InitialClientConfig> repo = new ClientRepositoryCollection<InitialClientConfig>(_logService, _clientAppService);
        _awaitableService.ForgetAwaitable(repo.Save(_config));
    }

    public void UpdateUserFlags(int flag, bool val)
    {
        if (user == null)
        {
            return;
        }
        if (val)
        {
            user.AddFlags(flag);
        }
        else
        {
            user.RemoveFlags(flag);
        }

        InitialClientConfig config = GetConfig();
        config.UserFlags = user.Flags;
        SaveConfig();
    }

}
