using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Dungeons.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.ProcGen.Settings.Texturse;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{
    public class OutdoorCrawlerMapTypeHelper : BaseCrawlerMapTypeHelper
    {

        ISamplingService _samplingService;
        ICrawlerMapService _mapService;
        ILineGenService _lineGenService;

        public override ECrawlerMapTypes GetKey() { return ECrawlerMapTypes.Outdoors; }

        public override async Awaitable<CrawlerMapRoot> Enter(PartyData partyData, EnterCrawlerMapData mapData, CancellationToken token)
        {
            partyData.MapId = mapData.MapId;
            partyData.MapX = mapData.MapX;
            partyData.MapZ = mapData.MapZ;
            partyData.MapRot = mapData.MapRot;
            string mapId = "Outdoors" + mapData.MapId;


            GameObject go = new GameObject() { name = "Outdoors" };
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

        public override int GetBlockingBits(CrawlerMapRoot mapRoot, int sx, int sz, int ex, int ez)
        {
            return 0;
        }


        private List<Color> _biomeColors = null;
        private List<Color> GetZoneColors()
        {
            if (_biomeColors == null)
            {
                _biomeColors = new List<Color>()
                {
                    Color.black, // City/POI
                    new Color(0,0.8f,0),  // Field
                    new Color(0,0.5f,0), // Forest
                    new Color(0.7f,0.7f,0), // Savannah
                    new Color(0.4f,0.7f,0), // Hills
                    new Color(0.5f,0.3f,0), // Swamp
                    new Color(1,1,1), // Snow
                    new Color(0.5f,0.5f,0.5f), // Mountain
                    new Color(1,1,0), // Desert
                    new Color(0.7f,0.2f,0), // Lava
                    new Color(0.3f,0.3f,1), // Water
                    new Color(0.75f,0.75f,0.75f), // Road
                };
            }
            return _biomeColors;
        }

        public override CrawlerMap Generate(CrawlerMapGenData genData)
        {



            CrawlerMap map = genData.World.CreateMap(genData);
            map.DungeonArt = _gameData.Get<DungeonArtSettings>(null).Get(map.DungeonArtId);
            IRandom rand = new MyRandom(genData.World.IdKey * 13 + map.IdKey*131);

            byte[,] overrides = new byte[map.Width, map.Height];
            long[,] terrain = new long[map.Width, map.Height];
            SpawnResult[,] objects = new SpawnResult[map.Width, map.Height];

            List<ZoneRegion> regions = new List<ZoneRegion>();

            List<ZoneType> allZones = _gameData.Get<ZoneTypeSettings>(null).GetData().OrderBy(x=>x.MinLevel).ToList();

            List<long> okZoneIds = allZones.Where(x => x.GenChance > 0).Select(x=>x.IdKey).ToList();

            List<Color> biomeColors = GetZoneColors();

            int startMapEdgeSize = 4;

            int cityDistanceFromEdge = startMapEdgeSize * 2;


            int fullRegionZones = allZones.Where(x => x.MinLevel <= 100).Count();

            SamplingData samplingData = new SamplingData()
            {
                Count = fullRegionZones,
                MaxAttemptsPerItem = 20,
                XMin = cityDistanceFromEdge,
                XMax = map.Width - cityDistanceFromEdge,
                YMin = cityDistanceFromEdge,
                YMax = map.Height - cityDistanceFromEdge,
                MinSeparation = 15,
                Seed = rand.Next(),
            };

            
            List<MyPoint2> points = _samplingService.PlanePoissonSample(samplingData);

            int sortx = (rand.NextDouble() < 0.5 ? -1 : 1);
            int sorty = (rand.NextDouble() < 0.5 ? -1 : 1);

            points = points.OrderBy(p=>p.X*sortx).ThenBy(p=>p.Y*sorty).ToList();

            List<MyPoint2> origPoints = new List<MyPoint2>(points);

            MyPoint2 firstPoint = points[0];

            points = points.OrderBy(p =>
                Math.Sqrt(
                    (p.X - firstPoint.X) * (p.X - firstPoint.X) +
                    (p.Y - firstPoint.Y) * (p.Y - firstPoint.Y)
                    )).ToList();


            int level = 1;
            int levelDelta = 7;
            float spreadDelta = 0.2f;
            float dirDelta = 0.3f;

            long cityZoneId = 0;
            long waterZoneId = allZones.FirstOrDefault(x => x.Name == "Water").IdKey;
            long roadZoneId = allZones.FirstOrDefault(x => x.Name == "Road").IdKey;
            long poiZoneId = allZones.FirstOrDefault(x => x.Name == "PointOfInterest").IdKey;
            long mountainZoneId = allZones.FirstOrDefault(x => x.Name == "Mountains").IdKey;

            while (points.Count > 0 && allZones.Count > 0)
            {
                List<ZoneType> okZones = allZones.Where(x => x.MinLevel <= level).ToList();

                if (okZones.Count < 1)
                {
                    break;
                }

                MyPoint2 centerPoint = points[0];

                points.Remove(centerPoint);

                ZoneType biomeType = okZones[rand.Next() % okZones.Count];

                allZones.Remove(biomeType);

                ZoneRegion region = new ZoneRegion()
                {
                    CenterX = (int)centerPoint.X,
                    CenterY = (int)centerPoint.Y,
                    SpreadX = MathUtils.FloatRange(1 - spreadDelta, 1 + spreadDelta, rand),
                    SpreadY = MathUtils.FloatRange(1 - spreadDelta, 1 + spreadDelta, rand),
                    ZoneTypeId = biomeType.IdKey,
                    DirX = MathUtils.FloatRange(-dirDelta, dirDelta, rand),
                    DirY = MathUtils.FloatRange(-dirDelta, dirDelta, rand),
                    Level = level,
                };

                level += levelDelta;

                regions.Add(region);

            }

            if (regions.Count < 1)
            {
                return map;
            }

            float radiusDelta = 0.2f;

            int radius = 0;
            while (true)
            {
                bool foundUnsetCell = false;
                for (int x = 0; x < map.Width; x++)
                {
                    for (int y = 0; y < map.Height; y++)
                    {
                        if (terrain[x,y] == 0)
                        {
                            foundUnsetCell = true;
                            break;
                        }
                    }
                    if (foundUnsetCell)
                    {
                        break;
                    }
                }

                if (!foundUnsetCell)
                {
                    break;
                }

                radius++;

                foreach (ZoneRegion region in regions)
                {
                    float currRadius = MathUtils.FloatRange(radius * (1 - radiusDelta), radius * (1 + radiusDelta), rand);

                    float xrad = currRadius * region.SpreadX;
                    float yrad = currRadius * region.SpreadY;
                    float xcenter = region.CenterX + region.DirX * currRadius;
                    float ycenter = region.CenterY * region.DirY * currRadius;

                    xcenter = region.CenterX;
                    ycenter = region.CenterY;

                    int xmin = MathUtils.Clamp(0, (int)(xcenter - xrad - 1), map.Width - 1);
                    int xmax = MathUtils.Clamp(0, (int)(xcenter + xrad + 1), map.Width - 1);

                    int ymin = MathUtils.Clamp(0, (int)(ycenter - yrad - 1), map.Height - 1);
                    int ymax = MathUtils.Clamp(0, (int)(ycenter + yrad + 1), map.Height - 1);

                    for (int x = xmin; x <= xmax; x++)
                    {
                        for (int y = ymin; y <= ymax; y++)
                        {

                            if (terrain[x,y] != 0)
                            {
                                continue;
                            }

                            float xpct = (x - xcenter)/xrad;
                            float ypct = (y - ycenter)/yrad;

                            float distScale = Mathf.Sqrt(xpct*xpct + ypct*ypct);

                            if (distScale <= 1)
                            {
                                terrain[x, y] = region.ZoneTypeId;
                            }
                        }
                    }
                }
            }



            List<float> cornerRadii = new List<float>();

            float minCornerRadius = 12;
            float maxCornerRadius = 20;

            for (int c = 0; c < 4; c++)
            {
                cornerRadii.Add(MathUtils.FloatRange(minCornerRadius, maxCornerRadius, rand));
            }


            int maxCheckRadius = (int)(maxCornerRadius + startMapEdgeSize);

            int xcorner = 0;
            int ycorner = 0;
            for (int x = 0; x < map.Width; x++)
            {
                
                for (int y = 0; y < map.Height; y++)
                {
                    int cornerIndex = -1;

                    if (x <= maxCheckRadius)
                    {
                        xcorner = 0;
                        if (y <= maxCheckRadius)
                        {
                            ycorner = 0;
                            cornerIndex = 0;
                        }
                        else if (y >= map.Height-maxCheckRadius-1)
                        { 
                            cornerIndex = 1;
                            ycorner = map.Height - 1;
                        }
                    }
                    else if (x >= map.Width-maxCheckRadius-1)
                    {
                        xcorner = map.Width - 1;
                        if (y <= maxCheckRadius)
                        {
                            cornerIndex = 2;
                            ycorner = 0;
                        }
                        else if (y >= map.Height-maxCheckRadius-1)
                        {
                            cornerIndex = 3;
                            ycorner = map.Height - 1;
                        }
                    }

                    int mapEdgeSize = startMapEdgeSize + MathUtils.IntRange(-1, 1, rand);
                    if ((x < mapEdgeSize || x >= map.Width - mapEdgeSize) ||
                        (y < mapEdgeSize || y >= map.Height - mapEdgeSize))
                    {
                        terrain[x, y] = waterZoneId;
                    }


                    if (cornerIndex >= 0 && cornerIndex < cornerRadii.Count)
                    {
                        int currRadius = (int)cornerRadii[cornerIndex]+startMapEdgeSize;


                        int cx = xcorner;
                        int cy = ycorner;

                        if (cx > 0)
                        {
                            cx -= currRadius;
                        }
                        else
                        {
                            cx += currRadius;
                        }

                        if (cy > 0)
                        {
                            cy -= currRadius;
                            
                        }
                        else
                        {
                            cy += currRadius;
                        }

                        if (cx < map.Width/2 && x > cx)
                        {
                            continue;
                        }

                        if (cx > map.Width/2 && x < cx)
                        {
                            continue;
                        }

                        if (cy < map.Height/2 && y > cy)
                        {
                            continue;
                        }

                        if (cy > map.Height/2 && y < cy)
                        {
                            continue;
                        }

                        float currDist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));

                        currDist += MathUtils.FloatRange(-1, 1, rand);

                        if (currDist >= currRadius && terrain[x, y] != waterZoneId)                
                        {
                            terrain[x, y] = waterZoneId;
                        }
                    }
                }
            }

            origPoints = origPoints.OrderBy(x => x.X).ToList();

            // Roads between cities

            List<MyPoint2> remainingPoints = new List<MyPoint2>(origPoints);


            MyPoint2 prevPoint = null;
            MyPoint2 currPoint = remainingPoints[0];
            remainingPoints.RemoveAt(0);

            while (remainingPoints.Count > 0)
            {
                MyPoint2 nextPoint = remainingPoints[0];
                remainingPoints.RemoveAt(0);

                int cx = (int)(currPoint.X);
                int cy = (int)(currPoint.Y);

                int nx = (int)(nextPoint.X);
                int ny = (int)(nextPoint.Y);


                int px = cx;

                if (prevPoint != null)
                {
                    px = (int)(prevPoint.X);    
                }

                bool foundRoad = false;
                // First checkbackwards to see if there's a matching road.
                for (int x = nx; x >= px; x--)
                {
                    if (terrain[x,ny] == roadZoneId)
                    {
                        foundRoad = true;
                        for (int xx = nx; xx > x; xx--)
                        {
                            if (terrain[xx, ny] == roadZoneId)
                            {
                                break;
                            }
                            terrain[xx,ny] = roadZoneId;

                        }
                    }
                }

                if (foundRoad)
                {

                    prevPoint = currPoint;
                    currPoint = nextPoint;
                    continue;
                }

                float dx = nx - cx;
                float dy = ny - cy;


                int midx = (int)(cx + (int)MathUtils.FloatRange(0.33f, 0.67f, rand) * dx);
                int midy = (int)(cy + (int)MathUtils.FloatRange(0.33f, 0.67f, rand)  * dy);

                List<int[]> segments = new List<int[]>();

                segments.Add(new int[] { cx, cy, midx, midy });
                segments.Add(new int[] { midx, midy, nx, ny });


                foreach (int[] segment in segments)
                {
                    int sx = segment[0];
                    int sy = segment[1];
                    int ex = segment[2];
                    int ey = segment[3];

                    if (rand.NextDouble() < 0.5f)
                    {
                        for (int xx = Math.Min(sx,ex); xx <= Math.Max(sx,ex); xx++)
                        {
                            terrain[xx, sy] = roadZoneId;
                        }
                        for (int yy = Math.Min(sy,ey); yy <= Math.Max(sy,ey); yy++)
                        {
                            terrain[ex, yy] = roadZoneId;
                        }
                    }
                    else
                    {
                        for (int xx = Math.Min(sx, ex); xx <= Math.Max(sx, ex); xx++)
                        {
                            terrain[xx, ey] = roadZoneId;
                        }
                        for (int yy = Math.Min(sy, ey); yy <= Math.Max(sy, ey); yy++)
                        {
                            terrain[sx, yy] = roadZoneId;
                        }
                    }
                }

                prevPoint = currPoint;
                currPoint = nextPoint;
            }



            // Rivers
            // Mountains at zone borders. (okZoneIds if  two diff make a small blob...only replacing things in ok biomeIds

            int crad = 1;
            int rrad = 2;
            int trad = Math.Max(crad, rrad);
            for (int x = trad; x < map.Width-trad; x++)
            {
                for (int y = trad; y < map.Height-trad; y++)
                {
                    List<long> currOkZoneIds = new List<long>();
                    bool foundBadZoneId = false;


                    // Check for roads.
                    for (int xx = x - rrad; xx <= x + rrad; xx++)
                    {
                        for (int yy = y - rrad; yy <= y + rrad; yy++)
                        {
                            if (terrain[xx, yy] == roadZoneId)
                            {
                                foundBadZoneId = true;
                                break;
                            }
                        }
                    }

                    if (foundBadZoneId)
                    {
                        continue;
                    }

                    // Now check smaller radius for diff biomes.
                    for (int xx = x-crad; xx <= x+crad; xx++)
                    {
                        for (int yy = y-crad; yy <= y+crad; yy++)
                        {
                            long tid = terrain[xx, yy];
                            if (okZoneIds.Contains(tid))
                            {
                                if (!currOkZoneIds.Contains(tid))
                                {
                                    currOkZoneIds.Add(tid);
                                }
                            }
                            else if (tid != mountainZoneId)
                            {
                                foundBadZoneId = true;
                                break;
                            }
                        }
                    }

                    int nrad = rand.NextDouble() < 0.2f ? 1 : 0;

                    if (!foundBadZoneId && currOkZoneIds.Count > 1)
                    {
                        for (int xx = x - nrad; xx <= x + nrad; xx++)
                        {
                            for (int yy = y - nrad; yy <= y + nrad; yy++)
                            {
                                terrain[xx, yy] = mountainZoneId;
                            }
                        }
                    }
                }
            }

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    map.CoreData[map.GetIndex(x,y)] = (byte)(terrain[x,y]);
                }
            }

            for (int c = 0; c < origPoints.Count; c++)
            {
                MyPoint2 pt = origPoints[c];


                int cityLevel = 1;
                ZoneRegion zoneRegion = regions.FirstOrDefault(x=>x.CenterX == pt.X && x.CenterY == pt.Y);  

                if (zoneRegion != null)
                {
                    cityLevel = (int)zoneRegion.Level;
                }

                terrain[(int)pt.X, (int)pt.Y] = cityZoneId;
                CrawlerMapGenData cityGenData = new CrawlerMapGenData()
                {
                    World = genData.World,
                    MapType = ECrawlerMapTypes.City,
                    Level = cityLevel,                
                    Width = MathUtils.IntRange(15,25,rand),
                    Height = MathUtils.IntRange(15,25,rand),
                    FromMapId = map.IdKey,
                    FromMapX = (int)(pt.X),
                    FromMapZ = (int)(pt.Y),
                };
                map.Details.Add(new MapCellDetail() { EntityTypeId = EntityTypes.Map, EntityId = -1, Value = cityLevel, X = (int)pt.X, Z = (int)pt.Y });
                CrawlerMap cityMap = _mapService.Generate(cityGenData);

                cityLevel += levelDelta;

                
            }

            try
            {

                Texture2D tex = new Texture2D(map.Width, map.Height, TextureFormat.RGB24, true, true);

                for (int x = 0; x < map.Width; x++)
                {
                    for (int y = 0; y < map.Height; y++)
                    {
                        tex.SetPixel(x, y, biomeColors[(int)terrain[x, y]]);
                    }
                }

                BinaryFileRepository repo = new BinaryFileRepository(_logService);
                repo.SaveBytes("CrawlerOutdoors.png", tex.EncodeToPNG());

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            return map;
        }



        public override async Awaitable DrawCell(CrawlerMapRoot mapRoot, UnityMapCell cell, int xpos, int zpos, CancellationToken token)
        {
            int bz = CrawlerMapConstants.BlockSize;

            if (cell.Content == null)
            {
                cell.Content = new GameObject() { name = "Cell" + cell.X + "." + cell.Z };
                GEntityUtils.AddToParent(cell.Content, mapRoot.gameObject);
                cell.Content.AddComponent<GImage>();
                Canvas canvas = cell.Content.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                RectTransform rectTransform = cell.Content.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector3(bz,bz);
                cell.Content.transform.eulerAngles = new Vector3(90, 0, 0);               
                cell.Content.transform.position = new Vector3(xpos * bz, 0, zpos * bz);
            }

            long biomeTypeId = mapRoot.Map.CoreData[mapRoot.Map.GetIndex(cell.X, cell.Z)];

            try
            {
                //AddWallComponent(mapRoot.Assets.Floor, cell.Content, new Vector3(0, 0, 0), new Vector3(90, 0, 0));
                if (biomeTypeId > 0)
                {
                    ZoneType biomeType = _gameData.Get<ZoneTypeSettings>(null).Get(biomeTypeId);

                    if (biomeType != null)
                    {
                        LoadTerrainTexture(cell.Content, biomeType.Textures.Where(x=>x.TextureChannelId == MapConstants.BaseTerrainIndex).First().TextureTypeId, token);
                    }
                }
            }
            catch (Exception e)
            {
                _logService.Info("Draw Cell Error: " + e.Message);
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
