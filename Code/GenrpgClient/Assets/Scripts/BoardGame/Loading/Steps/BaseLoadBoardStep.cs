using Assets.Scripts.BoardGame.Controllers;
using Assets.Scripts.BoardGame.Loading.Constants;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.Client.Assets.Services;
using Genrpg.Shared.Logging.Interfaces;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Loading
{

    public class BoardArtLoadData
    {
        public Vector3 Position;   
    }

    public abstract class BaseLoadBoardStep : ILoadBoardStep
    {
        protected IBoardGameController _controller;
        protected IAssetService _assetService;
        protected ILogService _logService;
        protected IClientEntityService _clientEntityService;
        public int Order => (int)GetKey();
        public abstract Awaitable Execute(BoardData boardData, CancellationToken token);
        public abstract ELoadBoardSteps GetKey();
    }
}
