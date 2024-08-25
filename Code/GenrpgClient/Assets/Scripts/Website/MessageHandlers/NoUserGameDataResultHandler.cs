using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Website.Messages.Login;
using Genrpg.Shared.Website.Messages.UploadMap;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spawns.Entities;
using System;
using System.Collections.Generic;
using UI.Screens.Constants;
using Genrpg.Shared.GameSettings.Interfaces;
using Assets.Scripts.GameSettings.Services;

using System.Threading;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.Website.Messages.NoUserGameData;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Characters.PlayerData;
using UnityEngine;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class NoUserGameDataResultHandler : BaseClientLoginResultHandler<NoUserGameDataResult>
    {
        private IScreenService _screenService;
        private IClientAuthService _loginService;
        private IAssetService _assetService;
        private IClientWebService _webNetworkService;
        private IClientGameDataService _gameDataService;
        protected override void InnerProcess(NoUserGameDataResult result, CancellationToken token)
        {
            AwaitableUtils.ForgetAwaitable(InnerProcessAsync(result, token));
        }

        private async Awaitable InnerProcessAsync(NoUserGameDataResult result, CancellationToken token)
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

            _screenService.Open(ScreenId.Crawler);


            while (_screenService.GetScreen(ScreenId.Crawler) == null)
            {
                await Awaitable.NextFrameAsync(token);
            }

            _screenService.CloseAll(new List<ScreenId>() { ScreenId.Crawler });
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
