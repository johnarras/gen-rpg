using System;
using System.Collections.Generic;
using System.Threading;
using Assets.Scripts.GameSettings.Services;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using UnityEngine;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using System.Threading.Tasks;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Messages;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.UI.Entities;
using Assets.Scripts.Awaitables;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Core.Interfaces;
using Genrpg.Shared.Accounts.WebApi.Login;
using Genrpg.Shared.Accounts.WebApi.Signup;


public interface IClientAuthService : IInitializable, IClientResetCleanup
{
    void StartAuth(CancellationToken token);
    void Logout();
    void ExitMap();
    Awaitable LoginToServer(LoginRequest command, CancellationToken token);
    Awaitable Signup(SignupRequest command, CancellationToken token);
    Awaitable SaveLocalUserData(string email, string loginToken);
    Awaitable StartNoUser(CancellationToken token);
}

public class ClientAuthService : IClientAuthService
{
    private const string LocalUserFilename = "LocalUser";

    private IClientWebService _clientWebService;
    private IRealtimeNetworkService _realtimeNetworkService;
    private IScreenService _screenService;
    private IMapTerrainManager _mapManager;
    private IClientMapObjectManager _objectManager;
    private IZoneGenService _zoneGenService;
    private IClientGameDataService _gameDataService;
    private IRepositoryService _repoService;
    private ILogService _logService;
    protected IGameData _gameData;
    protected IPlayerManager _playerManager;
    protected IMapProvider _mapProvider;
    protected IClientGameState _gs;
    private IClientConfigContainer _config;
    private IClientCryptoService _clientCryptoService;
    private IClientAppService _clientAppService;
    protected IAwaitableService _awaitableService;

    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    public void StartAuth(CancellationToken token)
    {
        LocalUserData localData = _repoService.Load<LocalUserData>(LocalUserFilename).Result;

        string userid = "";
        string email = "";
        string password = "";
        
        if (localData != null)
        {
            try
            {
                userid = localData.UserId;
                password = _clientCryptoService.DecryptString(localData.LoginToken);
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "StartLogin");
            }
        }
        if ((!string.IsNullOrEmpty(email) || !string.IsNullOrEmpty(userid)) && !string.IsNullOrEmpty(password))
        {
            LoginRequest LoginRequest = new LoginRequest()
            {
                UserId = userid,
                Email = email,
                Password = password,
                ClientVersion = _clientAppService.Version,
                DeviceId = _clientCryptoService.GetDeviceId(),
            };

            _awaitableService.ForgetAwaitable(LoginToServer(LoginRequest, token));
            _screenService.Open(ScreenId.Loading, true);
            return;
        }

        // Otherwise we either had no local login or we had no valid online login, and in this case
        // show the login screen.      
        _screenService.Open(ScreenId.Login, true);
        _screenService.Close(ScreenId.Loading);

    }

    public void Logout()
    {
        _logService.Info("Logging out");
        ExitMMOMap();
        _gs.user = null;
        _screenService.CloseAll();
        _screenService.Close(ScreenId.HUD);
        _screenService.Open(ScreenId.Login);
    }

    public void ExitMap()
    {
        _logService.Info("Exiting Map");
        ExitMMOMap();
        _screenService.CloseAll();
        _screenService.Close(ScreenId.HUD);
        _screenService.Open(ScreenId.CharacterSelect);
    }

    private void ExitMMOMap()
    {
        _zoneGenService.CancelMapToken();
        _playerManager.SetUnit(null);
        _realtimeNetworkService.CloseClient();
        _mapManager.Clear();
        _objectManager.Reset();
        UnityZoneGenService.LoadedMapId = null;
        _mapProvider.SetMap(null);
        _mapProvider.SetSpawns(null);
        _gs.ch = null;

    }
    public async Awaitable SaveLocalUserData(string userId, string loginToken)
    {
        LocalUserData localUserData = new LocalUserData()
        {
            Id = LocalUserFilename,
            UserId = userId,
            LoginToken = _clientCryptoService.EncryptString(loginToken),
        };

        await _repoService.Save(localUserData);
    }


    public async Awaitable LoginToServer(LoginRequest command, CancellationToken token)
    {
        command.AccountProductId = _config.Config.AccountProductId;

        try
        {
            await _gameDataService.LoadCachedSettings(_gs);
        }
        catch (Exception e)
        {
            _logService.Exception(e, "LoginToServer.LoadData");
        }

        List<ITopLevelSettings> allSettings = _gameData.AllSettings();

        command.ClientSettings = new List<ClientCachedGameSettings>();

        foreach (ITopLevelSettings settings in allSettings)
        {
            if (settings.Id.IndexOf(GameDataConstants.DefaultFilename) >= 0 && settings is IUpdateData updateData)
            {
                command.ClientSettings.Add(new ClientCachedGameSettings()
                {
                    ClientSaveTime = updateData.UpdateTime,
                    TypeName = settings.GetType().Name,
                });
            }
        }

        LoginResponse result = await _clientWebService.SendAuthWebRequestAsync<LoginResponse>(command, token);

        if (result == null)
        {
            _logService.Info("Got null result on send of " + command.GetType().Name);
        }
    }

    public async Awaitable StartNoUser(CancellationToken token)
    {
        try
        {
            await _gameDataService.LoadCachedSettings(_gs);
        }
        catch (Exception e)
        {
            _logService.Exception(e, "LoginToServer.LoadData");
        }
        LoginResponse result = new LoginResponse() { User = new User() { Id = "Local" } };

        WebServerResponseSet resultSet = new WebServerResponseSet() { Responses = new List<IWebResponse>() { result } };

        string txt = SerializationUtils.Serialize(resultSet);
        _clientWebService.HandleResponses(txt, null, token);
    }

    public async Awaitable Signup(SignupRequest command, CancellationToken token)
    {
        command.AccountProductId = _config.Config.AccountProductId;
        _clientWebService.SendAuthWebRequest(command, token);
        await Task.CompletedTask;
    }

    public async Task OnClientResetCleanup(CancellationToken token)
    {
        ExitMMOMap();
        await Task.CompletedTask;
    }
}