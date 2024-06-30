
using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.GameObjects;

using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Dungeons.Settings;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;
using System.Linq;
using Assets.Scripts.Crawler.Maps.Loading;
using Genrpg.Shared.Zones.Settings;
using System.Threading.Tasks;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using UnityEditor.iOS.Extensions.Common;
using Genrpg.Shared.Entities.Constants;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{
    public class CityCrawlerMapTypeHelper : BaseCrawlerMapTypeHelper
    {

        public override ECrawlerMapTypes GetKey() { return ECrawlerMapTypes.City; }

        public override async Awaitable<CrawlerMapRoot> Enter(PartyData partyData, EnterCrawlerMapData mapData, CancellationToken token)
        {
            partyData.MapId = mapData.MapId;
            partyData.MapX = mapData.MapX;
            partyData.MapZ = mapData.MapZ;
            partyData.MapRot = mapData.MapRot;
            string mapId = "City" + mapData.MapId;


            GameObject go = new GameObject() { name = "City" };
            CrawlerMapRoot mapRoot = _gameObjectService.GetOrAddComponent<CrawlerMapRoot>(go);
            mapRoot.SetupFromMap(mapData.Map);
            mapRoot.name = mapId;
            mapRoot.MapId = mapId;
            mapRoot.DrawX = partyData.MapX * CrawlerMapConstants.BlockSize;
            mapRoot.DrawZ = partyData.MapZ * CrawlerMapConstants.BlockSize;
            mapRoot.DrawY = CrawlerMapConstants.BlockSize / 2;
                

            await Task.CompletedTask;
            return mapRoot;
        }

        public override CrawlerMap Generate(CrawlerMapGenData genData)
        {
            MyRandom rand = new MyRandom(genData.World.IdKey*3 + genData.World.GetMaxMapId()*17);

            long cityZoneTypeId = _gameData.Get<ZoneTypeSettings>(null).GetData().FirstOrDefault(x => x.Name == "City")?.IdKey ?? 0;

            CrawlerMap map = genData.World.CreateMap(genData);

            map.SetupDataBlocks();

            long seed = map.IdKey % 1238921 + genData.World.IdKey;

            bool[,] clearCells = new bool[map.Width, map.Height];

            clearCells[map.Width / 2, map.Height / 2] = true;

            List<MyPoint> endPoints = new List<MyPoint> { new MyPoint(map.Width / 2, map.Height / 2) };

            int streetCount = (int)(Math.Sqrt((map.Width * map.Height)) * 0.75f);

            for (int times = 0; times < streetCount; times++)
            {
                MyPoint startPoint = endPoints[rand.Next() % endPoints.Count];

                List<MyPoint> okOffsets = new List<MyPoint>();

                for (int x = -1; x <= 1; x++)
                {
                    int sx = startPoint.X + x;

                    if (sx < 2 || sx >= map.Width - 2)
                    {
                        continue;
                    }

                    for (int y = -1; y <= 1; y++)
                    {
                        if ((x != 0) == (y != 0))
                        {
                            continue;
                        }

                        int sy = startPoint.Y + y;

                        if (sy < 2 || sy >= map.Height - 2)
                        {
                            continue;
                        }

                        if (clearCells[sx, sy] == true)
                        {
                            continue;
                        }

                        okOffsets.Add(new MyPoint(x, y));
                    }
                }

                if (okOffsets.Count < 1)
                {
                    continue;
                }

                MyPoint offset = okOffsets[rand.Next() % okOffsets.Count];


                int length = MathUtils.IntRange(3, 12, rand);

                for (int l = 1; l < length; l++)
                {
                    int lx = startPoint.X + l * offset.X;
                    int ly = startPoint.Y + l * offset.Y;

                    if (lx < 1 || lx >= map.Width - 1 ||
                        ly < 1 || ly >= map.Height - 1)
                    {
                        continue;
                    }

                    clearCells[lx, ly] = true;

                    endPoints.Add(new MyPoint(lx, ly));

                    if ((l == length / 2 && rand.NextDouble() < 0.2f) || l == length - 1 || rand.NextDouble() < 0.05f)
                    {
                        endPoints.Add(new MyPoint(lx, ly));
                    }
                }
            }

            int gateX = -1;
            int gateZ = -1;

            // X on border, y in middle.
            if (rand.NextDouble() < 0.5f)
            {
                gateX = (rand.NextDouble() < 0.5 ? 0 : map.Width - 1);
                gateZ = MathUtils.IntRange(map.Height / 3, map.Height * 2 / 3, rand);
                int start = gateX;                
                int dir = (gateX == 0 ? 1 : -1);

                int xx = start;
                while (true)
                {
                    if (clearCells[xx,gateZ])
                    {
                        break;
                    }
                    clearCells[xx, gateZ] = true;
                    xx += dir;
                    if (xx < 0 || xx >= map.Width)
                    {
                        break;
                    }
                }
            }
            else
            {
                gateZ = (rand.NextDouble() < 0.5 ? 0 : map.Height - 1);
                gateX = MathUtils.IntRange(map.Width/3, map.Width * 2 / 3, rand);
                int start = gateZ;
                int dir = (gateZ == 0 ? 1 : -1);

                int zz = start;
                while (true)
                {
                    if (clearCells[gateX,zz])
                    {
                        break;
                    }
                    clearCells[gateX,zz] = true;
                    zz += dir;
                    if (zz < 0 || zz >= map.Height)
                    {
                        break;
                    }
                }
            }


            map.Details.Add(new MapCellDetail()
            {
                EntityTypeId = EntityTypes.Map,
                EntityId = genData.FromMapId,
                ToX = genData.FromMapX,
                ToZ = genData.FromMapZ,
                X = gateX,
                Z = gateZ
            });

            CrawlerMap fromMap = genData.World.GetMap(genData.FromMapId);

            if (fromMap != null)
            {
                MapCellDetail fromDetail = fromMap.Details.FirstOrDefault(x=>x.X == genData.FromMapX && x.Z == genData.FromMapZ);  

                if (fromDetail != null)
                {
                    fromDetail.ToX = gateX;
                    fromDetail.ToZ = gateZ;
                    fromDetail.EntityId = map.IdKey;
                }
            }
            

            IReadOnlyList<BuildingType> buildings = _gameData.Get<BuildingSettings>(null).GetData();

            List<BuildingType> crawlerBuildings = buildings.Where(x => x.IsCrawlerBuilding).ToList();

            List<BuildingType> fillerBuildings = crawlerBuildings.Where(x => x.VariationCount > 1).ToList();

            List<BuildingType> requiredBuildings = crawlerBuildings.Where(x => x.VariationCount <= 1).ToList();


            List<MyPoint2> points = new List<MyPoint2>();

            if (fillerBuildings.Count > 0)
            {
                for (int xx = 0; xx < clearCells.GetLength(0); xx++)
                {
                    for (int yy = 0; yy < clearCells.GetLength(1); yy++)
                    {
                        if (clearCells[xx, yy])
                        {
                            continue;
                        }

                        List<MyPoint> okDirs = new List<MyPoint>();

                        for (int x = -1; x <= 1; x++)
                        {
                            int sx = xx + x;

                            if (sx < 0 || sx >= map.Width - 1)
                            {
                                continue;
                            }

                            for (int y = -1; y <= 1; y++)
                            {
                                if ((x != 0) == (y != 0))
                                {
                                    continue;
                                }

                                int sy = yy + y;

                                if (sy < 0 || sy >= map.Height)
                                {
                                    continue;
                                }

                                if (clearCells[sx, sy] == true)
                                {
                                    okDirs.Add(new MyPoint(x, y));
                                }
                            }
                        }

                        if (okDirs.Count > 0)
                        {
                            MyPoint okDir = okDirs[rand.Next() % okDirs.Count];

                            int dir = (okDir.X > 0 ? 0 : okDir.Y > 0 ? 3 : okDir.X < 0 ? 2 : 1);

                            dir = (dir + 1) % 4;

                            map.CoreData[map.GetIndex(xx, yy)] |= (byte)(dir << CrawlerMapConstants.CityDirBitShiftSize);

                            BuildingType btype = fillerBuildings[rand.Next() % fillerBuildings.Count];

                            map.ExtraData[map.GetIndex(xx, yy)] = (byte)(btype.IdKey);

                            points.Add(new MyPoint2(xx, yy));
                        }

                    }
                }
            }

            foreach (BuildingType btype in requiredBuildings)
            {
                if (points.Count < 1)
                {
                    break;
                }

                MyPoint2 currPoint = points[rand.Next() % points.Count];
                points.Remove(currPoint);
                map.ExtraData[map.GetIndex((int)currPoint.X, (int)currPoint.Y)] = (byte)btype.IdKey;
            }


            map.DungeonArt = _gameData.Get<DungeonArtSettings>(null).Get(map.DungeonArtId);

            return map;
        }


        public override int GetBlockingBits(CrawlerMapRoot mapRoot, int sx, int sz, int ex, int ez)
        {
            int extraData = mapRoot.Map.ExtraData[mapRoot.Map.GetIndex(ex, ez)];

            if (extraData == 0)
            {
                return WallTypes.None;
            }

            byte coreData = mapRoot.Map.CoreData[mapRoot.Map.GetIndex(ex, ez)];

            int dir = (int)(coreData >> CrawlerMapConstants.CityDirBitShiftSize);

            int angle = dir * 90;

            int dx = ex - sx;
            int dz = ez - sz;

            int moveAngle = ((dx == 0 ? (dz > 0 ? 90 : 270) : (dx > 0 ? 180 : 0)) + 90) % 360;

            if (moveAngle == angle)
            {
                return WallTypes.Building;
            }

            return extraData != 0 ? WallTypes.Wall : WallTypes.None;
        }


        public override async Awaitable DrawCell(CrawlerMapRoot mapRoot, UnityMapCell cell, int xpos, int zpos, CancellationToken token)
        {
            if (mapRoot.Assets == null)
            {
                return;
            }
            
            int bz = CrawlerMapConstants.BlockSize;

            if (cell.Content == null)
            {
                cell.Content = new GameObject() { name = "Cell" + cell.X + "." + cell.Z };
                GEntityUtils.AddToParent(cell.Content, mapRoot.gameObject);
                cell.Content.transform.position = new Vector3(xpos * bz, 0, zpos * bz);
            }


            long zoneTypeId = mapRoot.Map.CoreData[mapRoot.Map.GetIndex(cell.X, cell.Z)];

            ZoneType zoneType = null;

            AddWallComponent(mapRoot.Assets.Floor, cell.Content, new Vector3(0, 0, 0), new Vector3(90, 0, 0));
            if (zoneTypeId > 0)
            {
                zoneType = _gameData.Get<ZoneTypeSettings>(null).Get(zoneTypeId);

                if (zoneType != null && zoneType.Textures != null)
                {
                    ZoneTextureType zoneTextureType = zoneType.Textures.FirstOrDefault(x => x.TextureChannelId == MapConstants.BaseTerrainIndex);
                    if (zoneTextureType != null && zoneTextureType.TextureTypeId > 0)
                    {
                        LoadTerrainTexture(cell.Content, zoneTextureType.TextureTypeId, token);
                    }
                }
            }

            byte extraData = mapRoot.Map.ExtraData[mapRoot.Map.GetIndex(cell.X, cell.Z)];

            if (extraData > 0)
            {
                BuildingType btype = _gameData.Get<BuildingSettings>(null).Get(extraData);

                if (btype != null)
                {

                    string suffix = "";

                    if (btype.VariationCount > 1)
                    {
                        int indexVal = (cell.X * 13 + cell.Z * 41) % btype.VariationCount + 1;
                        suffix = indexVal.ToString();
                    }

                    byte coreData = mapRoot.Map.CoreData[mapRoot.Map.GetIndex(cell.X, cell.Z)];

                    int dir = (int)(coreData >> CrawlerMapConstants.CityDirBitShiftSize);

                    int angle = dir * 90;


                    CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
                    {
                        MapCell = cell,
                        BuildingType = btype,
                        Angle = angle,
                        MapRoot = mapRoot,
                    };


                    _assetService.LoadAssetInto(cell.Content, AssetCategoryNames.Buildings, "Default/" + btype.Art + suffix, OnDownloadBuilding, loadData, token);
                }
            }

            await Task.CompletedTask;
            return;
        }

       
    }
}
