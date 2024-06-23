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

using System.Threading;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.GameSettings;
using UnityEngine;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class LoginResultHandler : BaseClientLoginResultHandler<LoginResult>
    {
        private IScreenService _screenService;
        private IClientLoginService _loginService;
        private IAssetService _assetService;
        private IWebNetworkService _webNetworkService;
        private IClientGameDataService _gameDataService;
        protected override void InnerProcess(LoginResult result, CancellationToken token)
        {
            AwaitableUtils.ForgetAwaitable(InnerProcessAsync(result, token));
        }

        private async Awaitable InnerProcessAsync(LoginResult result, CancellationToken token)
        { 
            List<ScreenId> keepOpenScreens = new List<ScreenId>();
            if (_screenService.GetScreen(ScreenId.Signup) != null)
            {
                keepOpenScreens.Add(ScreenId.Signup);
            }
            if (_screenService.GetScreen(ScreenId.Login) != null)
            {
                keepOpenScreens.Add(ScreenId.Login);
            }

            if (result == null || result.User == null)
            {
                _screenService.CloseAll(keepOpenScreens);
                if (keepOpenScreens.Count < 1)
                {
                    _screenService.Open(ScreenId.Login);
                }
                return;
            }

            _gs.user = result.User;
            _gs.characterStubs = result.CharacterStubs;
            _gs.mapStubs = result.MapStubs;

            foreach (IGameSettings settings in result.GameData)
            {
               await _gameDataService.SaveSettings(settings);
            }

            List<ITopLevelSettings> loadedSettings = _gameData.AllSettings();

            _gameData.AddData(result.GameData);

            if (_gs.user != null && !String.IsNullOrEmpty(_gs.user.Id))
            {
                await _loginService.SaveLocalUserData(_gs.user.Id);
            }

            await Awaitable.NextFrameAsync(cancellationToken: token);
            await Awaitable.NextFrameAsync(cancellationToken: token);

            _screenService.CloseAll();
            _screenService.Close(ScreenId.HUD);
            _screenService.Open(ScreenId.CharacterSelect);

        }

        public async Awaitable RetryUploadMap(CancellationToken token)
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
