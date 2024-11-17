using Assets.Scripts.Awaitables;
using Assets.Scripts.BoardGame.Services;
using Assets.Scripts.BoardGame.Tiles;
using Assets.Scripts.GameObjects;
using Assets.Scripts.ProcGen.Components;
using Genrpg.Shared.BoardGame.Messages.RollDice;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MVC.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Controllers
{

    public interface IBoardGameController : IInitializable
    {
        GameObject GetBoardAnchor();
        void LoadCurrentBoard();
        void ShowDiceRoll(RollDiceResult result);
        void RollDice();
        void SetTiles(List<TileController> tiles);
        IReadOnlyList<TileController> GetTiles();
        TileController GetTile(long indeX);
    }

    public class BoardGameController : IBoardGameController
    {
        private IClientGameState _gs;
        private GameObject _boardAnchor;
        private ILoadBoardService _loadBoardService;
        private IShowDiceRollService _showDiceRollService;
        private IClientWebService _clientWebService;
        private IClientUpdateService _updateService;
        private ISingletonContainer _singletonContainer;
        protected IAwaitableService _awaitableService;

        private CancellationToken _gameToken;
        private CancellationTokenSource _boardSource;
        private ICameraController _cameraController;
        private IPlayerManager _playerManager;
        private IClientEntityService _clientEntityService;

        private List<IView> _viewList = new List<IView>();
        private List<TileController> _tiles = new List<TileController>();   

        

        public async Task Initialize(CancellationToken token)
        {
            _gameToken = token;
            RefreshBoardToken();

            _updateService.AddUpdate(this, OnUpdate, UpdateType.Regular, token);

            await Task.CompletedTask;
        }

        private void OnUpdate()
        {

            GameObject player = _playerManager.GetPlayerGameObject();

            if (player != null && _cameraController.GetMainCamera() != null && _tiles != null && _tiles.Count > 0)
            {
                _cameraController.GetMainCamera().transform.position = player.transform.position + new Vector3(-20, 20, -20);
                _cameraController.GetMainCamera().transform.LookAt(player.transform);
            }
        }

        public GameObject GetBoardAnchor()
        {
            if (_boardAnchor == null)
            {
                _boardAnchor = _singletonContainer.GetSingleton("BoardAnchor");
            }
            return _boardAnchor;
        }

        public void SetTiles(List<TileController> tiles)
        {
            _tiles = tiles.OrderBy(x=>x.MarkerPos.Index).ToList();  
        }

        public IReadOnlyList<TileController> GetTiles()
        {
            return _tiles;
        }

        public TileController GetTile(long index)
        {
            return _tiles.FirstOrDefault(x=>x.MarkerPos.Index == index);    
        }

        private void RefreshBoardToken()
        {
            _boardSource?.Cancel();
            _boardSource?.Dispose();
            _boardSource = CancellationTokenSource.CreateLinkedTokenSource(_gameToken);
            Clear();
        }

        private void Clear()
        {
            _tiles.Clear();
        }

        public void LoadCurrentBoard()
        {
            RefreshBoardToken();
            _awaitableService.ForgetAwaitable(LoadCurrentBoardAsync(_boardSource.Token));
        }

        private async Awaitable LoadCurrentBoardAsync(CancellationToken token)
        { 
            await _loadBoardService.LoadBoard(_gs.ch.Get<BoardData>(), token);
        }

        public void RollDice ()
        {
            _clientWebService.SendClientWebCommand(new RollDiceCommand(), _boardSource.Token);
        }

        public void ShowDiceRoll(RollDiceResult result)
        {
            _awaitableService.ForgetAwaitable(ShowDiceRollAsync(result,_boardSource.Token));
        }

        private async Awaitable ShowDiceRollAsync(RollDiceResult result, CancellationToken token)
        { 
            await _showDiceRollService.ShowDiceRoll(result, token);
        }

    }
}
