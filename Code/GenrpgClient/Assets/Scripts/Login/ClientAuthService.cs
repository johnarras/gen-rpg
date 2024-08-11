using System;
using System.Collections.Generic;
using System.Threading;

using Assets.Scripts.GameSettings.Services;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Messages.Login;
using UI.Screens.Constants;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Website.Messages.NoUserGameData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Core.Entities;
using System.Threading.Tasks;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.MapServer.Services;
using UnityEngine;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages.Signup;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Messages;


public interface IClientAuthService : IInitializable
{
    void StartAuth(CancellationToken token);
    void Logout();
    void ExitMap();
    Awaitable LoginToServer(LoginCommand command, CancellationToken token);
    Awaitable Signup(SignupCommand command, CancellationToken token);
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
    protected IUnityGameState _gs;

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
                password = CryptoUtils.DecryptString(localData.LoginToken);
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "StartLogin");
            }
        }
        if ((!string.IsNullOrEmpty(email) || !string.IsNullOrEmpty(userid)) && !string.IsNullOrEmpty(password))
        {
            LoginCommand loginCommand = new LoginCommand()
            {
                UserId = userid,
                Email = email,
                Password = password,
                ClientVersion = AppUtils.Version,
                DeviceId = CryptoUtils.GetDeviceId(),
            };

            AwaitableUtils.ForgetAwaitable(LoginToServer(loginCommand, token));
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
        InnerExitMap();
        _gs.user = null;
        _screenService.CloseAll();
        _screenService.Close(ScreenId.HUD);
        _screenService.Open(ScreenId.Login);
    }

    public void ExitMap()
    {
        _logService.Info("Exiting Map");
        InnerExitMap();
        _screenService.CloseAll();
        _screenService.Close(ScreenId.HUD);
        _screenService.Open(ScreenId.CharacterSelect);
    }

    private void InnerExitMap()
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

    public async Awaitable LoginToServer(LoginCommand command, CancellationToken token)
    {
        command.AccountProductId = _gs.Config.AccountProductId;

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

        _clientWebService.SendAuthWebCommand(command, token);
    }

    public async Awaitable SaveLocalUserData(string userId, string loginToken)
    {
        LocalUserData localUserData = new LocalUserData()
        {
            Id = LocalUserFilename,
            UserId = userId,
            LoginToken = CryptoUtils.EncryptString(loginToken),
        };

        await _repoService.Save(localUserData);
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
        LoginResult result = new LoginResult() { User = new User() { Id = "Local" } };

        LoginServerResultSet resultSet = new LoginServerResultSet() { Results = new List<IWebResult>() { result } };

        string txt = SerializationUtils.Serialize(resultSet);
        _clientWebService.HandleResults(txt, token);
    }

    public async Awaitable Signup(SignupCommand command, CancellationToken token)
    {
        command.AccountProductId = _gs.Config.AccountProductId;
        _clientWebService.SendAuthWebCommand(command, token);
        await Task.CompletedTask;
    }
}