using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.MapServer.Entities;
using System;
using System.Collections.Generic;
using Genrpg.Shared.GameSettings.Interfaces;
using Assets.Scripts.GameSettings.Services;

using System.Threading;
using Genrpg.Shared.Spawns.WorldData;
using UnityEngine;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Users.PlayerData;
using Assets.Scripts.GameSettings.Entities;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.Client.Assets.Services;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Accounts.WebApi.Login;
using Genrpg.Shared.MapServer.WebApi.UploadMap;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class LoginResultHandler : BaseClientLoginResultHandler<LoginResponse>
    {
        private IScreenService _screenService;
        private IClientAuthService _loginService;
        private IAssetService _assetService;
        private IClientWebService _webNetworkService;
        private IClientGameDataService _gameDataService;
        private IClientGameDataService _clientGameDataService;
        private ICrawlerService _crawlerService;
        protected override void InnerProcess(LoginResponse result, CancellationToken token)
        {
            _awaitableService.ForgetAwaitable(InnerProcessAsync(result, token));
        }

        private async Awaitable InnerProcessAsync(LoginResponse result, CancellationToken token)
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
            _gs.ch = new Character(_repoService) { Id = _gs.user.Id, UserId = _gs.user.Id, Name = "StubCharacter" };

            foreach (IUnitData unitData in result.UserData)
            {
                unitData.AddTo(_gs.ch);
            }

            CoreUserData userData = _gs.ch.Get<CoreUserData>();
            if(userData != null)
            {

            }

            foreach (IGameSettings settings in result.GameData)
            {
               await _gameDataService.SaveSettings(settings);
            }

            List<ITopLevelSettings> loadedSettings = _gameData.AllSettings();
            _gameData.AddData(result.GameData);
            if (_gameData is ClientGameData clientGameData)
            {
                clientGameData.SetFilteredObject(_gs.ch);
            }

            if (_gs.user != null && !String.IsNullOrEmpty(_gs.user.Id) && !string.IsNullOrEmpty(result.LoginToken))
            {
                await _loginService.SaveLocalUserData(_gs.user.Id, result.LoginToken);
            }

            await Awaitable.NextFrameAsync(cancellationToken: token);
            await Awaitable.NextFrameAsync(cancellationToken: token);

            if (GameModeUtils.IsPureClientMode(_gs.GameMode))
            {
                _screenService.Open(_crawlerService.GetCrawlerScreenId());
            }
            else
            {
                _screenService.CloseAll();
                _screenService.Close(ScreenId.HUD);
                _screenService.Open(ScreenId.CharacterSelect);
            }
        }

        public async Awaitable RetryUploadMap(CancellationToken token)
        {
            // Set the mapId you want to upload to here.
            string mapId = "1";

            UploadMapRequest comm = new UploadMapRequest();
            comm.Map = await _repoService.Load<Map>("UploadedMap");
            comm.SpawnData = await _repoService.Load<MapSpawnData>("UploadedSpawns");
            comm.Map.Id = mapId;
            comm.SpawnData.Id = mapId;
            comm.WorldDataEnv = _assetService.GetWorldDataEnv();
            _webNetworkService.SendClientUserWebRequest(comm, token);
        }
    }
}
