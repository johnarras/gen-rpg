using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Characters.PlayerData;
using System.Collections.Generic;
using Genrpg.Shared.MapServer.Entities;
using UnityEngine; // Needed

using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using Assets.Scripts.ProcGen.RandomNumbers;
using Assets.Scripts.GameSettings.Entities;
using Genrpg.Shared.DataStores.Entities;
using Assets.Scripts.Model;

public interface IUnityGameState : IGameState, IInjectable
{
    User user { get; set; }
    Character ch { get; set; }
    List<CharacterStub> characterStubs { get; set; }
    List<MapStub> mapStubs { get; set; }
    ClientConfig Config { get; }
    string LoginServerURL { get; set; }
    InitialClientConfig GetConfig();
    bool CrawlerMode { get; set; }
    void UpdateUserFlags(int flag, bool val);
}

public class UnityGameState : GameState, IInjectable, IUnityGameState
{
    public IMapGenData md { get; set; } = null;
    public User user { get; set; }
    public Character ch { get; set; }
    public bool CrawlerMode { get; set; } = false;
    public List<CharacterStub> characterStubs { get; set; }  = new List<CharacterStub>();
    public List<MapStub> mapStubs { get; set; } = new List<MapStub>(); 


    public ClientConfig Config { get; }

    public string LoginServerURL { get; set; }
    public string Version { get; set; }
    public string RealtimeHost { get; set; }
    public string RealtimePort { get; set; }

    private ILogService _logService;
    public UnityGameState()
    {
        Config = ClientConfig.Load();
        _logService = new ClientLogger(Config);
       
        IAnalyticsService analyticsService = new ClientAnalyticsService(Config);
        loc = new ServiceLocator(_logService, analyticsService, new ClientGameData());
        loc.Set<IDispatcher>(new Dispatcher());
        loc.Set<IUnityGameState>(this);
        loc.Set<IClientRandom>(new ClientRandom());
        loc.Set<IMapGenData>(new MapGenData());
        loc.Set<IRepositoryService>(new ClientRepositoryService(_logService));  
    }

    protected string ConfigFilename = "InitialClientConfig";
    protected InitialClientConfig _config = null;
    public InitialClientConfig GetConfig()
    {
        if (_config == null)
        {
            ClientRepositoryCollection<InitialClientConfig> repo = new ClientRepositoryCollection<InitialClientConfig>(_logService);
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

        ClientRepositoryCollection<InitialClientConfig> repo = new ClientRepositoryCollection<InitialClientConfig>(_logService);
        AwaitableUtils.ForgetAwaitable(repo.Save(_config));
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
