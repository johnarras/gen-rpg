using Assets.Scripts.BoardGame.Loading.Constants;
using Assets.Scripts.MapTerrain;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Zones.Settings;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Loading.Steps
{
    public class PaintTerrain : BaseLoadBoardStep
    {
        public override ELoadBoardSteps GetKey() { return ELoadBoardSteps.PaintTerrain; }


        private IMapTerrainManager _terrainManager;
        private IMapProvider _mapProvider;
        private ITerrainTextureManager _textureManager;
        private IZoneGenService _zoneGenService;

        public override async Awaitable Execute(BoardData boardData, CancellationToken token)
        {
            ZoneType ztype = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(boardData.ZoneTypeId);


            Map map = _mapProvider.GetMap();

            List<TerrainPatchData> patches = new List<TerrainPatchData>();
            for (int x = 0; x < map.BlockCount; x++)
            {
                for (int y = 0; y < BoardMapConstants.TerrainBlockCount; y++)
                {
                    patches.Add(_terrainManager.GetTerrainPatch(x, y));
                }
            }

            float[,] distances = _boardGameController.GetPathDistances();

            int asize = map.BlockCount * MapConstants.TerrainPatchSize;

            float[,,] alphas = new float[asize,asize, MapConstants.MaxTerrainIndex];


            int xoffset = 11+(int)boardData.Seed % 23;
            int yoffset = 13+(int)boardData.Seed % 119;

            for (int x = 0; x < asize; x++)
            {
                for (int y = 0; y < asize; y++)
                {
                    alphas[x, y, MapConstants.BaseTerrainIndex] = 1.0f;

                    float dist = distances[y,x];

                    int hash = (x * xoffset + y * yoffset) % 100;

                    if (dist == 0)
                    {
                        float pct = hash / 100.0f;

                        alphas[x, y, MapConstants.BaseTerrainIndex] = 0;
                        alphas[x, y, MapConstants.DirtTerrainIndex] = pct;
                        alphas[x, y, MapConstants.RoadTerrainIndex] = 1-pct;
                    }
                    else if (dist < BoardMapConstants.MaxDistanceFromPath)
                    {
                        float pct = 1.0f*dist/ BoardMapConstants.MaxDistanceFromPath;

                        
                        alphas[x, y, MapConstants.DirtTerrainIndex] = 1 - pct;
                        alphas[x, y, MapConstants.BaseTerrainIndex] = pct;

                    }
                }
            }


            
            foreach (TerrainPatchData patch in patches)
            {
                for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
                {
                    for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
                    {
                        patch.mainZoneIds[x, y] = (byte) patch.MainZoneIdList.First();
                    }
                }

                patch.baseAlphas = new float[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize, MapConstants.MaxTerrainIndex];


                int sx = patch.X * (MapConstants.TerrainPatchSize-1);
                int sy = patch.Y * (MapConstants.TerrainPatchSize-1);
                for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
                {
                    for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
                    {
                        for (int t = 0; t < MapConstants.MaxTerrainIndex; t++)
                        {
                            patch.baseAlphas[x, y, t] = alphas[x+sy,y+sx,t];
                        }
                    }
                }

                await _textureManager.SetOneTerrainPatchLayers(patch, token, true);
                await _zoneGenService.SetOnePatchAlphamaps(patch, token);
            }

            
            await Task.CompletedTask;
        }
    }
}
