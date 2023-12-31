using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Assets.Scripts.GameSettings.Services;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Login.Messages.Login;
using UI.Screens.Constants;
using GEntity = UnityEngine.GameObject;


public interface IClientLoginService : IService
{
    void StartLogin(UnityGameState gs, CancellationToken token);
    void Logout(UnityGameState gs);
    void ExitMap(UnityGameState gs);
    UniTask LoginToServer(UnityGameState gs, LoginCommand command, CancellationToken token);
    UniTask SaveLocalUserData(UnityGameState gs, string email);
}

public class ClientLoginService : IClientLoginService
{

    private const string LocalUserFilename = "LocalUser";

    private IWebNetworkService _webNetworkService;
    private IRealtimeNetworkService _realtimeNetworkService;
    private IScreenService _screenService;
    private IMapTerrainManager _mapManager;
    private IClientMapObjectManager _objectManager;
    private IZoneGenService _zoneGenService;
    private IClientGameDataService _gameDataService;
    private string _pwd = "";

    public void StartLogin(UnityGameState gs, CancellationToken token)
    {
        LocalUserData localData = gs.repo.Load<LocalUserData>(LocalUserFilename).Result;



        string email = "";
        string password = "";
        
        if (localData != null)
        {
            try
            {
                email = localData.Email;
                password = EncryptionUtils.DecryptString(localData.Password);

            }
            catch (Exception ex)
            {
                gs.logger.Exception(ex, "StartLogin");
            }
        }
        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        {
            LoginCommand loginCommand = new LoginCommand()
            {
                Email = email,
                Password = password,
            };

            LoginToServer(gs, loginCommand, token).Forget();
            _screenService.Open(gs, ScreenId.Loading, true);
            return;
        }

        // Otherwise we either had no local login or we had no valid online login, and in this case
        // show the login screen.      
        _screenService.Open(gs, ScreenId.Login, true);
        _screenService.Close(gs,ScreenId.Loading);

    }

    public void Logout(UnityGameState gs)
    {
        gs.logger.Info("Logging out");
        InnerExitMap(gs);
        gs.user = null;
        gs.data = null;
        _screenService.CloseAll(gs);
        _screenService.Close(gs, ScreenId.HUD);
        _screenService.Open(gs, ScreenId.Login);
    }

    public void ExitMap(UnityGameState gs)
    {
        gs.logger.Info("Exiting Map");
        InnerExitMap(gs);
        _screenService.CloseAll(gs);
        _screenService.Close(gs, ScreenId.HUD);
        _screenService.Open(gs, ScreenId.CharacterSelect);
    }

    private void InnerExitMap(UnityGameState gs)
    {
        _zoneGenService.CancelMapToken();
        PlayerObject.Destroy();
        _realtimeNetworkService.CloseClient();
        _mapManager.Clear(gs);
        _objectManager.Reset();
        UnityZoneGenService.LoadedMapId = null;
        if (gs.md != null)
        {
            gs.map = null;
            gs.md = null;
            gs.spawns = null;
        }
        gs.ch = null;

    }

    public async UniTask LoginToServer(UnityGameState gs, LoginCommand command, CancellationToken token)
    {
        _pwd = command.Password;

        try
        {
            await _gameDataService.LoadCachedSettings(gs);
        }
        catch (Exception e)
        {
            gs.logger.Exception(e, "LoginToServer.LoadData");
        }

        List<IGameSettings> allSettings = gs.data.GetAllData();

        command.ClientSettings = new List<ClientCachedGameSettings>();

        foreach (IGameSettings settings in allSettings)
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

        _webNetworkService.SendLoginWebCommand(command, token);

        await UniTask.CompletedTask;
    }

    public async UniTask SaveLocalUserData(UnityGameState gs, string email)
    {
        LocalUserData localUserData = new LocalUserData()
        {
            Id = LocalUserFilename,
            Email = gs.user.Email,
            Password = EncryptionUtils.EncryptString(_pwd),
        };

        await gs.repo.Save(localUserData);
    }
}