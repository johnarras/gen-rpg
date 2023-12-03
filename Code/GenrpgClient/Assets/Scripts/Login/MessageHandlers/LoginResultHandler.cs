using Assets.Scripts.Login.Messages.Core;
using System.Threading.Tasks;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Login.Messages.Login;
using Genrpg.Shared.Login.Messages.UploadMap;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spawns.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using UI.Screens.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Assets.Scripts.GameSettings.Services;
using Genrpg.Shared.AI.Entities;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class LoginResultHandler : BaseClientLoginResultHandler<LoginResult>
    {
        private IScreenService _screenService;
        private IClientLoginService _loginService;
        private IAssetService _assetService;
        private INetworkService _networkService;
        private IClientGameDataService _gameDataService;
        protected override void InnerProcess(UnityGameState gs, LoginResult result, CancellationToken token)
        {
            TaskUtils.AddTask(InnerProcessAsync(gs, result, token), "loginresultinnerprocess",token);
        }

        private async Task InnerProcessAsync(UnityGameState gs, LoginResult result, CancellationToken token)
        { 
            List<ScreenId> keepOpenScreens = new List<ScreenId>();
            if (_screenService.GetScreen(gs, ScreenId.Signup) != null)
            {
                keepOpenScreens.Add(ScreenId.Signup);
            }
            if (_screenService.GetScreen(gs, ScreenId.Login) != null)
            {
                keepOpenScreens.Add(ScreenId.Login);
            }

            if (result == null || result.User == null)
            {
                _screenService.CloseAll(gs, keepOpenScreens);
                if (keepOpenScreens.Count < 1)
                {
                    _screenService.Open(gs, ScreenId.Login);
                }
                return;
            }

            gs.user = result.User;
            gs.characterStubs = result.CharacterStubs;
            gs.mapStubs = result.MapStubs;

            foreach (IGameSettings settings in result.GameData)
            {
               await _gameDataService.SaveSettings(gs, settings);
            }

            List<IGameSettings> loadedSettings = gs.data.GetAllData();

            FloatingTextScreen.Instance.ShowMessage("Cache Qty: " + loadedSettings.Count + " D/L Qty: " + result.GameData.Count);

            gs.data.AddData(result.GameData);

            if (gs.user != null && !String.IsNullOrEmpty(gs.user.Id))
            {
                await _loginService.SaveLocalUserData(gs, gs.user.Email);
            }

            await Task.Delay(1);
            await Task.Delay(1);

            _screenService.CloseAll(gs);
            _screenService.Close(gs, ScreenId.HUD);
            _screenService.Open(gs, ScreenId.CharacterSelect);

            string env = gs.Env;

            //await RetryUploadMap(gs, token);
        }

        public async Task RetryUploadMap(UnityGameState gs, CancellationToken token)
        {
            string mapId = "1";

            UploadMapCommand comm = new UploadMapCommand();
            comm.Map = await gs.repo.Load<Map>(mapId);
            comm.SpawnData = await gs.repo.Load<MapSpawnData>(mapId);

            _networkService.SendClientWebCommand(comm, token);
        }
    }
}
