
using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Cysharp.Threading.Tasks;
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

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{
    public class CityCrawlerMapTypeHelper : BaseCrawlerMapTypeHelper
    {

        public override ECrawlerMapTypes GetKey() { return ECrawlerMapTypes.City; }

        public override async UniTask<CrawlerMapRoot> Enter(UnityGameState gs, PartyData partyData, EnterCrawlerMapData mapData, CancellationToken token)
        {
            partyData.MapId = mapData.MapId;
            partyData.MapX = mapData.MapX;
            partyData.MapZ = mapData.MapZ;
            partyData.MapRot = mapData.MapRot;
            string mapId = "City" + mapData.MapId;

            CrawlerMap cmap = GenerateCityMap(gs, partyData, mapData.MapId);

            GameObject go = new GameObject() { name = "City" };
            CrawlerMapRoot mapRoot = GEntityUtils.GetOrAddComponent<CrawlerMapRoot>(gs, go);
            mapRoot.SetupFromMap(cmap);
            mapRoot.name = mapId;
            mapRoot.MapId = mapId;
            mapRoot.DrawX = partyData.MapX * CrawlerMapConstants.BlockSize;
            mapRoot.DrawZ = partyData.MapZ * CrawlerMapConstants.BlockSize;
            mapRoot.DrawY = CrawlerMapConstants.BlockSize / 2;

            await UniTask.CompletedTask;
            return mapRoot;
        }

        CrawlerMap GenerateCityMap(UnityGameState gs, PartyData party, long cityId)
        {
            CrawlerMap cmap = new CrawlerMap();
            cmap.Looping = false;
            cmap.MapType = ECrawlerMapTypes.City;
            MyRandom rand = new MyRandom(StrUtils.GetIdHash(cityId + "City"));

            int minSize = 20;
            int maxSize = 30;

            cmap.XSize = MathUtils.IntRange(minSize, maxSize, rand);
            cmap.ZSize = MathUtils.IntRange(minSize, maxSize, rand);

            cmap.SetupDataBlocks();

            long seed = party.Seed % 1238921 + cityId;

            bool[,] clearCells = new bool[cmap.XSize, cmap.ZSize];

            clearCells[cmap.XSize / 2, cmap.ZSize / 2] = true;

            List<MyPoint> endPoints = new List<MyPoint> { new MyPoint(cmap.XSize / 2, cmap.ZSize / 2) };

            int streetCount = (int)(Math.Sqrt((cmap.XSize * cmap.ZSize)) * 0.75f);

            _logService.Info("StreetCount: " + streetCount);

            for (int times = 0; times < streetCount; times++)
            {
                MyPoint startPoint = endPoints[rand.Next() % endPoints.Count];

                List<MyPoint> okOffsets = new List<MyPoint>();

                for (int x = -1; x <= 1; x++)
                {
                    int sx = startPoint.X + x;

                    if (sx < 2 || sx >= cmap.XSize - 2)
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

                        if (sy < 2 || sy >= cmap.ZSize - 2)
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

                    if (lx < 1 || lx >= cmap.XSize - 1 ||
                        ly < 1 || ly >= cmap.ZSize - 1)
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

            int cityBiomeId = 0;

            List<ZoneType> zoneTypes = _gameData.Get<ZoneTypeSettings>(null).GetData().Where(x => x.IdKey > 0).ToList();

            for (int x = 0; x < cmap.XSize; x++)
            {
                for (int z = 0; z < cmap.ZSize; z++)
                {
                    long zoneTypeId = cityBiomeId;
                    if (rand.NextDouble() < 0.3f && zoneTypes.Count > 0)
                    {
                        zoneTypeId = zoneTypes[rand.Next() % zoneTypes.Count].IdKey;
                    }

                    cmap.CoreData[cmap.GetIndex(x, z)] = (byte)zoneTypeId;
                }
            }


            IReadOnlyList<BuildingType> buildings = _gameData.Get<BuildingSettings>(null).GetData();

            List<BuildingType> crawlerBuildings = buildings.Where(x => x.IsCrawlerBuilding).ToList();

            List<BuildingType> fillerBuildings = crawlerBuildings.Where(x => x.ArtCount > 1).ToList();

            List<BuildingType> requiredBuildings = crawlerBuildings.Where(x => x.ArtCount <= 1).ToList();

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

                            if (sx < 1 || sx >= cmap.XSize - 1)
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

                                if (sy < 1 || sy >= cmap.ZSize - 1)
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

                            cmap.CoreData[cmap.GetIndex(xx, yy)] |= (byte)(dir << CrawlerMapConstants.CityDirBitShiftSize);

                            BuildingType btype = fillerBuildings[rand.Next() % fillerBuildings.Count];

                            cmap.ExtraData[cmap.GetIndex(xx, yy)] = (byte)(btype.IdKey);
                        }

                    }
                }
            }
            cmap.DungeonArt = _gameData.Get<DungeonArtSettings>(null).Get(cmap.DungeonArtId);

            return cmap;
        }


        public override int GetBlockingBits(UnityGameState gs, CrawlerMapRoot mapRoot, int sx, int sz, int ex, int ez)
        {
            return mapRoot.Map.ExtraData[mapRoot.Map.GetIndex(ex, ez)] != 0 ? WallTypes.Wall : WallTypes.None;
        }


        public override async UniTask DrawCell(UnityGameState gs, CrawlerMapRoot mapRoot, UnityMapCell cell, int xpos, int zpos, CancellationToken token)
        {
            if (mapRoot.Assets == null)
            {
                return;
            }
            await UniTask.CompletedTask;
            int bz = CrawlerMapConstants.BlockSize;

            if (cell.Content == null)
            {
                cell.Content = new GameObject() { name = "Cell" + cell.X + "." + cell.Z };
                GEntityUtils.AddToParent(cell.Content, mapRoot.gameObject);
                cell.Content.transform.position = new Vector3(xpos * bz, 0, zpos * bz);
            }


            long zoneTypeId = mapRoot.Map.CoreData[mapRoot.Map.GetIndex(cell.X, cell.Z)];

            ZoneType zoneType = null;

            AddWallComponent(gs, mapRoot.Assets.Floor, cell.Content, new Vector3(0, 0, 0), new Vector3(90, 0, 0));
            if (zoneTypeId > 0)
            {
                zoneType = _gameData.Get<ZoneTypeSettings>(null).Get(zoneTypeId);

                if (zoneType != null && zoneType.Textures != null)
                {
                    ZoneTextureType zoneTextureType = zoneType.Textures.FirstOrDefault(x => x.TextureChannelId == MapConstants.BaseTerrainIndex);
                    if (zoneTextureType != null && zoneTextureType.TextureTypeId > 0)
                    {
                        LoadTerrainTexture(gs, cell.Content, zoneTextureType.TextureTypeId, token);
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

                    if (btype.ArtCount > 1)
                    {
                        int indexVal = (cell.X * 13 + cell.Z * 41) % btype.ArtCount + 1;
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


                    _assetService.LoadAssetInto(gs, cell.Content, AssetCategoryNames.Buildings, "Default/" + btype.Art + suffix, OnDownloadBuilding, loadData, token);
                }
            }

            return;
        }

       
    }
}
