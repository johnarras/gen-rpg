using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.MapServer.Entities;
using System;
using System.Collections.Generic;
using Genrpg.Shared.GameSettings.Interfaces;
using Assets.Scripts.GameSettings.Services;

using System.Threading;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.Website.Messages.NoUserGameData;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Characters.PlayerData;
using UnityEngine;
using Genrpg.Shared.Client.Assets;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.Client.Assets.Services;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.MapServer.WebApi.UploadMap;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class NoUserGameDataResultHandler : BaseClientLoginResultHandler<NoUserGameDataResponse>
    {
        private IScreenService _screenService;
        private IClientAuthService _loginService;
        private IAssetService _assetService;
        private IClientWebService _webNetworkService;
        private IClientGameDataService _gameDataService;
        private ICrawlerService _crawlerService;
        protected override void InnerProcess(NoUserGameDataResponse result, CancellationToken token)
        {
            _awaitableService.ForgetAwaitable(InnerProcessAsync(result, token));
        }

        private async Awaitable InnerProcessAsync(NoUserGameDataResponse result, CancellationToken token)
        {
            _gs.user = new User() { Id = "Crawler" };
            _gs.ch = new Character(_repoService) { Id = _gs.user.Id, UserId = _gs.user.Id };
            _gs.characterStubs = new List<CharacterStub>();
            _gs.mapStubs = new List<MapStub>();

            foreach (IGameSettings settings in result.GameData)
            {
               await _gameDataService.SaveSettings(settings);
            }

            if (_gs.user != null && !String.IsNullOrEmpty(_gs.user.Id))
            {                
                LocalUserData localUserData = await _repoService.Load<LocalUserData>(_gs.user.Id);
                string loginToken = localUserData != null ? localUserData.LoginToken : "Crawler";
                await _loginService.SaveLocalUserData(_gs.user.Id, loginToken);
            }

            _gameData.AddData(result.GameData);

            await Awaitable.NextFrameAsync(cancellationToken: token);
            await Awaitable.NextFrameAsync(cancellationToken: token);

            ScreenId screenId = ScreenId.CrawlerMainMenu;
            _screenService.Open(screenId);

            while (_screenService.GetScreen(screenId) == null)
            {
                await Awaitable.NextFrameAsync(token);
            }

            _screenService.CloseAll(new List<ScreenId>() { screenId });
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
