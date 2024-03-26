using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Login.Messages.Login;
using Genrpg.Shared.Login.Messages.UploadMap;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spawns.Entities;
using System;
using System.Collections.Generic;
using UI.Screens.Constants;
using Genrpg.Shared.GameSettings.Interfaces;
using Assets.Scripts.GameSettings.Services;
using Cysharp.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Spawns.WorldData;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class LoginResultHandler : BaseClientLoginResultHandler<LoginResult>
    {
        private IScreenService _screenService;
        private IClientLoginService _loginService;
        private IAssetService _assetService;
        private IWebNetworkService _webNetworkService;
        private IClientGameDataService _gameDataService;
        protected override void InnerProcess(UnityGameState gs, LoginResult result, CancellationToken token)
        {
            InnerProcessAsync(gs, result, token).Forget();
        }

        private async UniTask InnerProcessAsync(UnityGameState gs, LoginResult result, CancellationToken token)
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

            List<ITopLevelSettings> loadedSettings = gs.data.AllSettings();

            gs.data.AddData(result.GameData);

            if (gs.user != null && !String.IsNullOrEmpty(gs.user.Id))
            {
                await _loginService.SaveLocalUserData(gs, gs.user.Id);
            }

            await UniTask.NextFrame( cancellationToken: token);
            await UniTask.NextFrame( cancellationToken: token);

            _screenService.CloseAll(gs);
            _screenService.Close(gs, ScreenId.HUD);
            _screenService.Open(gs, ScreenId.CharacterSelect);

        }

        public async UniTask RetryUploadMap(UnityGameState gs, CancellationToken token)
        {
            // Set the mapId you want to upload to here.
            string mapId = "1";

            UploadMapCommand comm = new UploadMapCommand();
            comm.Map = await _repoService.Load<Map>("UploadedMap");
            comm.SpawnData = await _repoService.Load<MapSpawnData>("UploadedSpawns");
            comm.Map.Id = mapId;
            comm.SpawnData.Id = mapId;
            comm.WorldDataEnv = _assetService.GetWorldDataEnv();
            _webNetworkService.SendClientWebCommand(comm, token);
        }
    }
}
