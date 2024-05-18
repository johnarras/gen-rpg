using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.ProcGen.Settings.MapWater;
using Genrpg.Shared.ProcGen.Settings.Trees;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.ProcGen.Loading.Utils
{
    public interface IAddPoolService : IInjectable
    {
        bool TryAddPool(UnityGameState gs, WaterGenData genData);
    }

    public class AddPoolService : IAddPoolService
    {
        private IGameData _gameData = null;

        protected List<TreeType> waterPlants = null;

        public bool TryAddPool(UnityGameState gs, WaterGenData genData)
        {
            if (genData == null ||
                genData.x < MapConstants.TerrainPatchSize ||
                genData.z < MapConstants.TerrainPatchSize ||
                genData.x > gs.map.GetHwid() - MapConstants.TerrainPatchSize ||
                genData.z > gs.map.GetHhgt() - MapConstants.TerrainPatchSize)
            {
                return false;
            }

            if (waterPlants == null)
            {
                waterPlants = _gameData.Get<TreeTypeSettings>(gs.ch).GetData().Where(x => x.HasFlag(TreeFlags.IsWaterItem)).ToList();
            }

            if (genData.stepSize < 1)
            {
                genData.stepSize = 1;
            }

            int nx = genData.x + 0;
            int nz = genData.z + 0;

            int cx = genData.x;
            int cz = genData.z;


            if (gs.md.mapObjects[nx, nz] != 0)
            {
                bool foundOkPosition = false;
                for (int xx = nx - 1; xx <= nx + 1; xx++)
                {
                    for (int zz = nz - 1; zz <= nz + 1; zz++)
                    {
                        if (gs.md.mapObjects[xx, zz] == 0)
                        {
                            nz = zz;
                            nx = xx;
                            foundOkPosition = true;
                            break;
                        }
                    }
                    if (foundOkPosition)
                    {
                        break;
                    }
                }

                if (!foundOkPosition)
                {
                    return false;
                }

            }

            int xSizeMaxHeight = 0;
            int ySizeMaxHeight = 0;
            float maxHeightDiff = 0;

            int extraEdge = 4;

            float centerHeight = gs.md.heights[cx, cz] * MapConstants.MapHeight;

            float minHeightTotal = centerHeight + 3;





            for (int xsize = genData.minXSize; xsize <= genData.maxXSize; xsize += genData.stepSize)
            {
                for (int ysize = genData.minZSize; ysize <= genData.maxZSize; ysize += genData.stepSize)
                {


                    bool nearWater = false;
                    float minHeightAroundEdges = MapConstants.MapHeight;

                    float minHeightAnywhere = MapConstants.MapHeight;
                    for (int xx = cx - xsize - extraEdge; xx <= cx + xsize + extraEdge; xx++)
                    {
                        if (xx < 0 || xx >= gs.map.GetHwid() || nearWater)
                        {
                            minHeightAroundEdges = 0;
                            minHeightAnywhere = 0;
                            continue;
                        }

                        float dx = (xx - cx) * 1.0f / (xsize);
                        float ddx = dx * dx;
                        for (int yy = cz - ysize - extraEdge; yy <= cz + ysize + extraEdge; yy++)
                        {

                            if (yy < 0 || yy >= gs.map.GetHwid())
                            {
                                minHeightAroundEdges = 0;
                                minHeightAnywhere = 0;
                                continue;
                            }

                            if (FlagUtils.IsSet(gs.md.flags[xx, yy], MapGenFlags.NearWater))
                            {
                                nearWater = true;
                                break;
                            }

                            float dy = (yy - cz) * 1.0f / (ysize);
                            float ddy = dy * dy;


                            float currHeight = gs.md.heights[xx, yy] * MapConstants.MapHeight;
                            if (currHeight < minHeightAnywhere)
                            {
                                minHeightAnywhere = currHeight;
                            }
                            if (ddx + ddy >= 1)
                            {
                                if (currHeight < minHeightAroundEdges)
                                {
                                    minHeightAroundEdges = currHeight;
                                }
                            }
                        }
                    }


                    if (genData.maxHeight > 0 && minHeightAroundEdges > genData.maxHeight)
                    {
                        minHeightAroundEdges = genData.maxHeight;
                    }


                    if (minHeightAroundEdges < minHeightAnywhere + 2)
                    {
                        continue;
                    }



                    int heightDiff = (int)((minHeightAroundEdges - MapConstants.MinLandHeight));



                    if (heightDiff > maxHeightDiff)
                    {
                        xSizeMaxHeight = ysize;
                        ySizeMaxHeight = xsize;
                        maxHeightDiff = heightDiff;
                    }
                }
            }

            int nearWaterRad = 2;

            if (xSizeMaxHeight > 0 && ySizeMaxHeight > 0 && maxHeightDiff > 1)
            {
                float waterHeight = (MapConstants.MinLandHeight + (int)maxHeightDiff - 0.5f) / MapConstants.MapHeight;
                float maxPlantHeight = waterHeight + 0.5f / MapConstants.MapHeight;

                MyRandom rand = new MyRandom(genData.x * 31 + genData.z * 71);

                float plantChance = MathUtils.FloatRange(0, 1, rand);

                for (int xx = cx - xSizeMaxHeight - extraEdge; xx <= cx + xSizeMaxHeight + extraEdge; xx++)
                {
                    if (xx < 0 || xx >= gs.map.GetHwid())
                    {
                        continue;
                    }
                    for (int yy = cz - ySizeMaxHeight - extraEdge; yy <= cz + ySizeMaxHeight + extraEdge; yy++)
                    {
                        if (yy < 0 || yy >= gs.map.GetHhgt())
                        {
                            continue;
                        }

                        gs.md.flags[xx, yy] |= MapGenFlags.NearWater;
                        if (gs.md.heights[xx, yy] < waterHeight)
                        {
                            gs.md.flags[xx, yy] |= MapGenFlags.BelowWater;
                        }


                        int tx = xx + 0 * (xx / (MapConstants.TerrainPatchSize - 1));
                        int ty = yy + 0 * (yy / (MapConstants.TerrainPatchSize - 1));
                        int lowerObject = gs.md.mapObjects[tx, ty] & (1 << 16);

                        if (lowerObject < MapConstants.BridgeObjectOffset ||
                            lowerObject > MapConstants.BridgeObjectOffset + MapConstants.MapObjectOffsetMult)
                        {
                            if (gs.md.heights[xx, yy] < waterHeight)
                            {
                                //gs.md.mapObjects[tx,ty] = 0;
                            }
                            else
                            {

                                float dxpct = 1.0f * (cx - xx) / xSizeMaxHeight;
                                float dypct = 1.0f * (cz - yy) / ySizeMaxHeight;

                                float dpct = dxpct * dxpct + dypct * dypct;
                                if (dpct <= 1.0f)
                                {
                                    int ux = tx + 0 * (xx / (MapConstants.TerrainPatchSize - 1));
                                    int uy = ty + 0 * (yy / (MapConstants.TerrainPatchSize - 1));
                                    if (gs.md.mapObjects[ux, uy] == 0 && waterPlants.Count > 0 &&
                                        gs.md.heights[xx, yy] < maxPlantHeight && rand.NextDouble() < plantChance)
                                    {
                                        bool nearRealWater = false;
                                        for (int x1 = xx - nearWaterRad; x1 <= xx + nearWaterRad; x1++)
                                        {
                                            if (nearRealWater)
                                            {
                                                break;
                                            }

                                            if (x1 < 0 || x1 >= gs.map.GetHwid())
                                            {
                                                continue;
                                            }

                                            for (int y1 = yy - nearWaterRad; y1 <= yy + nearWaterRad; y1++)
                                            {
                                                if (y1 < 0 || y1 >= gs.map.GetHhgt())
                                                {
                                                    continue;
                                                }

                                                if (gs.md.heights[x1, y1] < waterHeight)
                                                {
                                                    nearRealWater = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (nearRealWater)
                                        {
                                            TreeType plantChosen = waterPlants[rand.Next() % waterPlants.Count];
                                            gs.md.mapObjects[ux, uy] = (int)(MapConstants.TreeObjectOffset + plantChosen.IdKey);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                gs.md.mapObjects[nx, nz] = MapConstants.WaterObjectOffset + (int)maxHeightDiff;
                int szOffset = (int)(xSizeMaxHeight * 256) + ySizeMaxHeight;
                gs.md.mapObjects[nx, nz] += (szOffset << 16);
                return true;
            }
            return false;
        }

    }
}
