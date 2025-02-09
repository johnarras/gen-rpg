using Assets.Scripts.BoardGame.Loading.Constants;
using Genrpg.Shared.BoardGame.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Loading
{
    public class CleanOldBoard : BaseLoadBoardStep
    {
        public override ELoadBoardSteps GetKey() {  return ELoadBoardSteps.CleanOldBoard;}
        private IMapGenData _mapGenData;
        private IMapTerrainManager _terrainManager;
        private ICameraController _cameraController;
        public async Awaitable Execute(BoardData boardData)
        {
            await Task.CompletedTask;
        }

        public override async Awaitable Execute(BoardData boardData, CancellationToken token)
        {
            _mapGenData.HaveSetHeights = false;
            _terrainManager.ClearPatches();
            _cameraController.SetupForBoardGame();
            _clientEntityService.DestroyAllChildren(_boardGameController.GetBoardAnchor());
            await Task.CompletedTask;
        }
    }
}
