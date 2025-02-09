using Assets.Scripts.BoardGame.Loading.Constants;
using Assets.Scripts.BoardGame.Tiles;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.Tiles.Settings;
using Genrpg.Shared.Utils.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Loading
{
    public class LoadTiles : BaseLoadBoardStep
    {
        protected IMapTerrainManager _terrainManager;
        private IMapGenData _mapGenData;
        public override ELoadBoardSteps GetKey() { return ELoadBoardSteps.LoadTiles; }

        public override async Awaitable Execute(BoardData boardData, CancellationToken token)
        {
            GameObject parent = base._boardGameController.GetBoardAnchor();

            int[,] pathGrid = new int[boardData.Width, boardData.Height];

            for (int x = 0; x < pathGrid.GetLength(0); x++)
            {
                for (int z = 0; z < pathGrid.GetLength(1); z++)
                {
                    pathGrid[x, z] = -1;
                }
            }

            float offset = 1.5f;
            PointXZ startPos = boardData.GetStartPos();

            bool isClockwise = boardData.IsClockwise();


            int currTileIndex = BoardGameConstants.FirstTileIndex;
            pathGrid[startPos.X, startPos.Z] = currTileIndex++;

            int currX = startPos.X;
            int currZ = startPos.Z;

            int xdelta = (isClockwise ? -1 : 1);
            int zdelta = (isClockwise ? 1 : -1);

            IReadOnlyList<TileType> tileTypes = _gameData.Get<TileTypeSettings>(_gs.ch).GetData();

            for (int t = 0; t < boardData.Length; t++)
            {
                if (boardData.IsOnPath(currX + xdelta, currZ) &&
                    pathGrid[currX+xdelta, currZ] < BoardGameConstants.FirstTileIndex)
                {
                    pathGrid[currX + xdelta, currZ] = currTileIndex++;
                    currX += xdelta;
                    continue;
                }
                if (boardData.IsOnPath(currX, currZ + zdelta) &&
                    pathGrid[currX, currZ + zdelta] < BoardGameConstants.FirstTileIndex)
                {
                    pathGrid[currX, currZ + zdelta] = currTileIndex++;
                    currZ += zdelta;
                    continue;
                }
                if (boardData.IsOnPath(currX - xdelta, currZ) &&
                    pathGrid[currX - xdelta, currZ] < BoardGameConstants.FirstTileIndex)
                {
                    pathGrid[currX - xdelta, currZ] = currTileIndex++;
                    currX -= xdelta;
                    continue;
                }
                if (boardData.IsOnPath(currX, currZ - zdelta) &&
                    pathGrid[currX, currZ - zdelta] < BoardGameConstants.FirstTileIndex)
                {
                    pathGrid[currX, currZ - zdelta] = currTileIndex++;
                    currZ -= zdelta;
                    continue;
                }
            }

            List<TileTypeWithIndex> ttlist = new List<TileTypeWithIndex>();

            for (int x = 0; x < pathGrid.GetLength(0); x++)
            {
                for (int z =0; z < pathGrid.GetLength(1); z++)
                {
                    int tileIndex = pathGrid[x, z];
                    if (tileIndex >= BoardGameConstants.FirstTileIndex)
                    {
                        TileTypeWithIndex tti = new TileTypeWithIndex()
                        {
                            GridX = x,
                            GridZ = z,
                            Index = tileIndex,
                            TileType = tileTypes.FirstOrDefault(x => x.IdKey == boardData.Tiles.Get(tileIndex)),
                            XPos = BoardMapConstants.StartPos+x*BoardMapConstants.CellSize+offset,
                            ZPos = BoardMapConstants.StartPos+z*BoardMapConstants.CellSize+offset,
                           
                        };
                        ttlist.Add(tti);
                    }
                }
            }

            foreach (TileTypeWithIndex tti in ttlist)
            {
                _boardGameController.SetTile(tti, token);
            }

            _boardGameController.SetPathGrid(pathGrid);

            while (_boardGameController.GetTiles().Count != boardData.Length)
            {
                await Awaitable.NextFrameAsync(token);
            }

            await Task.CompletedTask;
        }
    }
}
