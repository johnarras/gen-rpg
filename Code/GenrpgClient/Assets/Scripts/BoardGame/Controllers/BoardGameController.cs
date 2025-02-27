using Assets.Scripts.Awaitables;
using Assets.Scripts.BoardGame.Loading;
using Assets.Scripts.BoardGame.Loading.Constants;
using Assets.Scripts.BoardGame.Services;
using Assets.Scripts.BoardGame.Tiles;
using Assets.Scripts.GameObjects;
using Assets.Scripts.Lockouts.Constants;
using Assets.Scripts.Lockouts.Services;
using Assets.Scripts.MVC;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.WebApi.RollDice;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Client.Assets.Services;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.MVC.Interfaces;
using Genrpg.Shared.Tiles.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Controllers
{

    public interface IBoardGameController : IInitializable
    {
        GameObject GetBoardAnchor();
        void LoadCurrentBoard();
        void ShowDiceRoll(RollDiceResponse result);
        void RollDice();
        void SetTiles(List<TileController> tiles);
        void SetTile(TileTypeWithIndex ttwi, CancellationToken token);
        void SetTileTypeId(long tileTypeId, int index, CancellationToken token);
        IReadOnlyList<TileController> GetTiles();
        TileController GetTile(long indeX);

        public void SetPathGrid(int[,] pathGrid);
        public int[,] GetPathGrid();
        public float[,] GetPathDistances();
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
        private IGameData _gameData;
        private ILockoutManager _lockoutManager;
        private IClientWebService _webService;

        private ICameraController _cameraController;
        private IPlayerManager _playerManager;
        private IClientEntityService _clientEntityService;
        private IAssetService _assetService;
        private IMapTerrainManager _terrainManager;
        private ILogService _logService;

        private CancellationToken _token;
        private CancellationTokenSource _boardSource;

        private List<IView> _viewList = new List<IView>();
        private List<TileController> _tiles = new List<TileController>();

        private int[,] _pathGrid;
        private float[,] _pathDistances;

        public async Task Initialize(CancellationToken token)
        {
            _token = token;
            RefreshBoardToken();

            _updateService.AddUpdate(this, OnUpdate, UpdateTypes.Regular, token);

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
            _tiles = tiles.OrderBy(x=>x.GeTTileIndex()).ToList();  
        }

        public IReadOnlyList<TileController> GetTiles()
        {
            return _tiles;
        }

        public TileController GetTile(long index)
        {
            return _tiles.FirstOrDefault(x=>x.GeTTileIndex() == index);    
        }

        private void RefreshBoardToken()
        {
            _boardSource?.Cancel();
            _boardSource?.Dispose();
            _boardSource = CancellationTokenSource.CreateLinkedTokenSource(_token);
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
            if (_lockoutManager.HasLock(LockoutTypes.RollDice))
            {
                return;
            }
            _webService.SendClientUserWebRequest(new RollDiceRequest(), _token);
            _lockoutManager.AddLock(LockoutTypes.RollDice, RollDiceLocks.SendRequest);
        }

        public void ShowDiceRoll(RollDiceResponse result)
        {
            _awaitableService.ForgetAwaitable(ShowDiceRollAsync(result,_boardSource.Token));
        }

        private async Awaitable ShowDiceRollAsync(RollDiceResponse result, CancellationToken token)
        {
            await _showDiceRollService.ShowDiceRoll(result, token);
        }
        public void SetTile(TileTypeWithIndex ttwi, CancellationToken token)
        {

            _awaitableService.ForgetAwaitable(SetTileAsync(ttwi, token));
        }

        private async Awaitable SetTileAsync(TileTypeWithIndex tti, CancellationToken token)
        { 
            TileController newController = await _assetService.CreateAsync<TileController, TileTypeWithIndex>(tti, AssetCategoryNames.Tiles, tti.TileType.Art, GetBoardAnchor(),
            token);
            BaseView bv = newController.GetView() as BaseView;

            TileController currController = GetTile(tti.Index);

            if (currController != null)
            {
                BaseView bv2 = currController.GetView() as BaseView;
                _clientEntityService.Destroy(bv2.gameObject);
                _tiles.Remove(currController);
            }
            if (bv != null)
            {
                bv.name = bv.name + tti.Index;
                float height = _terrainManager.GetInterpolatedHeight(tti.XPos, tti.ZPos);
                bv.transform.position = new Vector3(tti.XPos, height + 0.3f, tti.ZPos);

                _tiles.Add(newController);
            }
        }

        public void SetTileTypeId(long tileTypeId, int index, CancellationToken token)
        {
            TileController controller = GetTile(index);

            if (controller == null)
            {
                return;
            }

            controller.GetModel().TileType = _gameData.Get<TileTypeSettings>(_gs.user).Get(tileTypeId);
            SetTile(controller.GetModel(), token);
        }

        public void SetPathGrid(int[,] pathGrid)
        {
            _pathGrid = pathGrid;
            CalcPathDistances(pathGrid);
        }

        public int[,] GetPathGrid()
        {
            return _pathGrid;
        }

        public float[,] GetPathDistances()
        {
            return _pathDistances;
        }

        private void CalcPathDistances(int[,] pathGrid)
        {
            int cellSize = BoardMapConstants.CellSize;

            BoardData boardData = _gs.ch.Get<BoardData>();

            int distSize = BoardMapConstants.TerrainBlockCount * MapConstants.TerrainPatchSize;
            _pathDistances = new float[distSize, distSize];


            int offset = 0;
            int startX = BoardMapConstants.StartPos - offset;
            int endX = startX + pathGrid.GetLength(0) * BoardMapConstants.CellSize + offset;
            int startY = BoardMapConstants.StartPos-offset;
            int endY = startY + pathGrid.GetLength(1) * BoardMapConstants.CellSize + offset;


            for (int x =0; x < _pathDistances.GetLength(0); x++)
            {
                for (int y = 0; y < _pathDistances.GetLength(1); y++)
                {
                    if (x < startX || x >= endX || y < startY || y >= endY)
                    {
                        _pathDistances[x, y] = BoardMapConstants.MaxDistanceFromPath;
                    }
                    else
                    {

                        float gxf = 1.0f * (x - startX) / cellSize;
                        float gyf = 1.0f * (y - startY) / cellSize;
                        int gx = (int)gxf;
                        int gy = (int)gyf;

                        float dx = gxf - gx;
                        float dy = gyf - gy;

                        float dist = Mathf.Sqrt(dx * dx + dy * dy);

                        if (gx >= 0 && gx < pathGrid.GetLength(0) && gy >= 0 && gy < pathGrid.GetLength(1) && 
                            pathGrid[gx,gy] >= BoardGameConstants.FirstTileIndex)
                        {
                            _pathDistances[gx, gy] = dist;
                        }
                        else
                        {
                            _pathDistances[x+offset,y+offset] = BoardMapConstants.MaxDistanceFromPath;
                        }
                    }
                }
            }

            while (true)
            {
                bool changedSomething = false;

                for (int x = 0; x < _pathDistances.GetLength(0); x++)
                {
                    for (int y = 0; y < _pathDistances.GetLength(1); y++)
                    {
                        for (int xx = x-1; xx <= x+1; xx++)
                        {
                            if (xx < 0 || xx >= _pathDistances.GetLength(0))
                            {
                                continue;
                            }

                            for (int yy = y-1; yy <= y+1; yy++)
                            {
                                if (yy < 0 || yy >= _pathDistances.GetLength(1))
                                {
                                    continue;
                                }

                                if ((xx == 0) != (yy == 0))
                                {
                                    continue;
                                }

                                float newVal = _pathDistances[xx, yy] + 1;

                                if (newVal < _pathDistances[x,y])
                                {
                                    _pathDistances[x,y] = newVal;
                                    changedSomething = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (!changedSomething)
                {
                    break;
                }
            }
        }

    }
}
