using Assets.Scripts.Login.Messages.Core;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.GameDatas;
using Genrpg.Shared.Login.Messages.Login;
using Genrpg.Shared.Login.Messages.UploadMap;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UI.Screens.Constants;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class LoginResultHandler : BaseClientLoginResultHandler<LoginResult>
    {
        private IScreenService _screenService;
        private IClientLoginService _loginService;
        private IAssetService _assetService;
        private INetworkService _networkService;
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
            gs.data = new GameData();
            foreach (BaseGameData baseGameData in result.Data)
            {
                baseGameData.Set(gs.data);
            }

            if (gs.user != null && !String.IsNullOrEmpty(gs.user.Id))
            {
                await _loginService.SaveLocalUserData(gs, gs.user.Email);
            }

            while (!_assetService.IsInitialized(gs))
            {
                await UniTask.NextFrame();
            }

            await UniTask.DelayFrame(5);

            _screenService.CloseAll(gs);
            _screenService.Close(gs, ScreenId.HUD);
            _screenService.Open(gs, ScreenId.CharacterSelect);

            string env = gs.Env;

            //await RetryUploadMap(gs, token);
        }

        public async UniTask RetryUploadMap(UnityGameState gs, CancellationToken token)
        {
            string mapId = "1";

            UploadMapCommand comm = new UploadMapCommand();
            comm.Map = await gs.repo.Load<Map>(mapId);
            comm.SpawnData = await gs.repo.Load<MapSpawnData>(mapId);

            _networkService.SendClientWebCommand(comm, token);
        }
    }
}
