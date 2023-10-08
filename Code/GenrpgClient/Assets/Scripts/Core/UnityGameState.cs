using GEntity = UnityEngine.GameObject;
using GComponent = UnityEngine.MonoBehaviour;
using ClientEvents;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Characters.Entities;
using System.Collections.Generic;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.ProcGen.Entities;
using System.Linq;
using UnityEngine; // Needed

public class UnityGameState : GameState
{
    public GEntity initObject = null;
	private InitClient initComponent = null;
    public MapGenData md = null;
    public User user { get; set; }
    public Character ch { get; set; }
    public List<CharacterStub> characterStubs = new List<CharacterStub>();
    public List<MapStub> mapStubs = new List<MapStub>();


    public List<GenZone> GenZones { get; set; } = new List<GenZone>();


    public UnityGameState()
    {
    }

    public GenZone GetGenZone(long zoneId)
    {
        GenZone genZone = GenZones.FirstOrDefault(x => x.IdKey == zoneId);
        if (genZone == null)
        {
            genZone = new GenZone() { IdKey = zoneId };
            GenZones.Add(genZone);
        }
        return genZone;
    }

    public string Env { get; set; }
    public string SiteURL { get; set; }
    public string ArtURL { get; set; } = InitClient.DefaultAssetPrefix;
    public string Version { get; set; }
    public string RealtimeHost { get; set; }
    public string RealtimePort { get; set; }

    public void SetInitObject(GEntity go)
	{
		if (go == null)
		{
			return;
		}
		initObject = go;
		initComponent = go.GetComponent<InitClient>();
        AddEvent<NewVersionEvent>(initComponent, OnNewVersion);
	}

	public T AddComponent<T> () where T : MonoBehaviour
	{

		if (initObject == null)
		{
			return default(T);
		}

        return GEntityUtils.GetOrAddComponent<T>(this, initObject);

	}

	public T GetComponent<T>() where T : MonoBehaviour
	{
		if (initObject != null)
		{
			return initObject.GetComponent<T>();
		}
		return default(T);
	}

    public T Dispatch<T> (T data) where T : class
    {
        return _dispatcher.DispatchEvent<T>(this,data);
	}
	

	private NewVersionEvent OnNewVersion(GameState gs, NewVersionEvent newVersion)
	{
		Caching.ClearCache ();
        return null;
	}



    protected string ConfigFilename = "InitialClientConfig";
    protected InitialClientConfig _config = null;
    public InitialClientConfig GetConfig()
    {
        if (_config == null)
        {

            ClientRepository<InitialClientConfig> repo = new ClientRepository<InitialClientConfig>(logger);
            _config = repo.Load(ConfigFilename).Result;
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

        ClientRepository<InitialClientConfig> repo = new ClientRepository<InitialClientConfig>(logger);
        repo.Save(_config).Wait();
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

    // The Dispatcher is hidden inside of gamestate to avoid wonky 
    // constructions every time it's called.

    private Dispatcher _dispatcher = new Dispatcher();

    public Dispatcher GetDispatcher()
    {
        return _dispatcher;
    }

    public void SetDispatcher(Dispatcher disp)
    {
        if (disp != null)
        {
            _dispatcher = disp;
        }
    }

    public void AddEvent<T>(GComponent c, GameAction<T> action) where T : class
    { 
        _dispatcher.AddEvent<T>(action);
        c.GetCancellationToken().Register(() => { _dispatcher.RemoveEvent(action); });
    }

    public void ClearDispatchEvents()
    {
        _dispatcher.Clear();
    }
}
