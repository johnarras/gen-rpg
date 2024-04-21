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
using Genrpg.Shared.Login.Messages.NoUserGameData;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Characters.PlayerData;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class NoUserGameDataResultHandler : BaseClientLoginResultHandler<NoUserGameDataResult>
    {
        private IScreenService _screenService;
        private IClientLoginService _loginService;
        private IAssetService _assetService;
        private IWebNetworkService _webNetworkService;
        private IClientGameDataService _gameDataService;
        protected override void InnerProcess(UnityGameState gs, NoUserGameDataResult result, CancellationToken token)
        {
            InnerProcessAsync(gs, result, token).Forget();
        }

        private async UniTask InnerProcessAsync(UnityGameState gs, NoUserGameDataResult result, CancellationToken token)
        {
            gs.user = new User() { Id = "Crawler" };
            gs.characterStubs = new List<CharacterStub>();
            gs.mapStubs = new List<MapStub>();

            foreach (IGameSettings settings in result.GameData)
            {
               await _gameDataService.SaveSettings(gs, settings);
            }
            if (gs.user != null && !String.IsNullOrEmpty(gs.user.Id))
            {
                await _loginService.SaveLocalUserData(gs, gs.user.Id);
            }

            _gameData.AddData(result.GameData);

            await UniTask.NextFrame( cancellationToken: token);
            await UniTask.NextFrame( cancellationToken: token);

            _screenService.Open(gs, ScreenId.Crawler);


            while (_screenService.GetScreen(gs, ScreenId.Crawler) == null)
            {
                await UniTask.NextFrame(token);
            }

            _screenService.CloseAll(gs, new List<ScreenId>() { ScreenId.Crawler });
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
