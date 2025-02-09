using Assets.Scripts.BoardGame.Controllers;
using Assets.Scripts.BoardGame.Loading.Constants;
using Assets.Scripts.BoardGame.Tiles;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.ProcGen.Settings.Trees;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Loading.Steps
{
    public class AddBoardTrees : BaseLoadBoardStep
    {

        protected IMapProvider _mapProvier;
        protected IBoardGameController _boardController;
        protected IMapTerrainManager _mapTerrainManager;

        public override ELoadBoardSteps GetKey() { return ELoadBoardSteps.AddTrees; }
        public override async Awaitable Execute(BoardData boardData, CancellationToken token)
        {
            Map map = _mapProvier.GetMap();

            GameObject tileRoot = _boardController.GetBoardAnchor();

            IReadOnlyList<TileController> tiles = _boardController.GetTiles();

            bool[,] blocked = new bool[map.GetHwid(), map.GetHhgt()];

            int maxMainPathIndex = -1;

            MyRandom rand = new MyRandom(boardData.Seed / 2 + 3313);

            int skipSize = 6;
            int offsetSize = skipSize - 1;

            ZoneType ztype = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(boardData.ZoneTypeId);

            if (ztype == null || ztype.TreeTypes.Count < 1)
            {
                return;
            }

            int maxTreeTypes = 3;
            List<ZoneTreeType> currentTypes = new List<ZoneTreeType>(ztype.TreeTypes);

            while (currentTypes.Count > maxTreeTypes)
            {
                currentTypes.RemoveAt(rand.Next(currentTypes.Count));
            }

            float currentTreeChance = 0.6f;
            currentTypes = currentTypes.OrderBy(x=>HashUtils.NewGuid()).ToList();       

            float minx = 100000;
            float maxx = -100000;
            float minz = 100000;
            float maxz = -100000;

            for (int i = 0; i < boardData.Length; i++)
            {
                if (boardData.GetPathIndex(i) == BoardGameConstants.StartPathIndex)
                {
                    maxMainPathIndex++;
                }
                else
                {
                    break;
                }
            }

            foreach (TileController tile in tiles)
            {
                Vector3 pos = (Vector3) tile.GetView().Position();
                int cx = (int)pos.x;
                int cz = (int)pos.z;

                int radius = 3;
                if (tile.GeTTileIndex() <= maxMainPathIndex)
                {
                    minx = Math.Min(cx, minx);
                    maxx = Math.Max(cx, maxx);
                    minz = Math.Min(cz, minz);
                    maxz = Math.Max(cz, maxz);
                }

                
                for (int xx = cx-radius; xx <= cx+radius; xx++)
                {
                    if (xx < 0 || xx >= map.GetHwid())
                    {
                        continue;
                    }

                    for (int zz = cz-radius; zz <= cz+radius; zz++)
                    {
                        if (zz < 0 || zz >= map.GetHhgt())
                        {
                            continue;
                        }

                        blocked[xx, zz] = true;
                    }
                }
            }

            TreeTypeSettings treeSettings = _gameData.Get<TreeTypeSettings>(_gs.ch);

            List<TreeType> finalTreeTypes = new List<TreeType>();

            foreach (ZoneTreeType ztt in currentTypes)
            {
                TreeType ttype = treeSettings.Get(ztt.TreeTypeId);  
                if (ttype != null)
                {
                    finalTreeTypes.Add(ttype);
                }
            }

            if (finalTreeTypes.Count < 1)
            {
                return;
            }

            for (int x = skipSize; x < map.GetHwid()-skipSize; x += skipSize)
            {
               for (int z = skipSize; z < map.GetHhgt()-skipSize; z += skipSize)
                {
                    int xx = x + MathUtils.IntRange(-offsetSize, offsetSize, rand);
                    int zz = z + MathUtils.IntRange(-offsetSize, offsetSize, rand); 

                    if (rand.NextDouble() > Math.Min(0.8f,ztype.TreeDensity*1.5f))
                    {
                        continue;
                    }

                    if (blocked[xx,zz])
                    {
                        continue;
                    }
                    if (xx >= minx && xx <= maxx && zz >= minz && zz <= maxz)
                    {
                        continue;
                    }

                    TreeType chosenType = finalTreeTypes[0];

                    for (int i = 0; i < finalTreeTypes.Count; i++)
                    {
                        if (rand.NextDouble() < currentTreeChance)
                        {
                            chosenType = finalTreeTypes[i];
                            break;
                        }
                    }


                    float height = _mapTerrainManager.GetInterpolatedHeight(xx, zz);

                    BoardArtLoadData artData = new BoardArtLoadData()
                    {
                        Position = new Vector3(xx,height,zz),

                    };

                    _assetService.LoadAssetInto(tileRoot, AssetCategoryNames.Trees, chosenType.Art + 
                        (1 + rand.Next() % chosenType.VariationCount), OnLoadTree, artData, token);
                }
            }
            await Task.CompletedTask;
        }

        private void OnLoadTree(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;
            if (go ==null)
            {
                return;
            }

            BoardArtLoadData artData = data as BoardArtLoadData;


            if (artData == null)
            {
                return;
            }

            go.transform.position = artData.Position;
        }
    }
}
