using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Dungeons.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.ProcGen.Settings.Trees;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Constants;
using Genrpg.Shared.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using Genrpg.Shared.Riddles.Settings;

namespace Assets.Scripts.Crawler.Maps.Services.GenerateMaps
{
    public class OutdoorCrawlerMapGenHelper : BaseCrawlerMapGenHelper
    {

        ISamplingService _samplingService;
        ILineGenService _lineGenService;
        ILootGenService _lootGenService;

        public override long GetKey() { return CrawlerMapTypes.Outdoors; }


        private List<UnityEngine.Color> _biomeColors = null;
        private List<UnityEngine.Color> GetZoneColors()
        {
            if (_biomeColors == null)
            {
                _biomeColors = new List<UnityEngine.Color>()
                {
                    UnityEngine.Color.black, // City/POI
                    new UnityEngine.Color(0,0.8f,0),  // Field
                    new UnityEngine.Color(0,0.5f,0), // Forest
                    new UnityEngine.Color(0.4f,0.7f,0), // Hills
                    new UnityEngine.Color(0.5f,0.3f,0), // Swamp
                    new UnityEngine.Color(0.8f,0.2f,0), // Badlands
                    new UnityEngine.Color(1,1,0), // Desert
                    new UnityEngine.Color(0.2f,0.2f,0.2f), // DarkForest
                    new UnityEngine.Color(0.7f,0.7f,0), // Savannah
                    new UnityEngine.Color(0.3f,0.3f,1), // Water
                    new UnityEngine.Color(0.5f,0.5f,0.5f), // Mountains
                    new UnityEngine.Color(1,1,1), // Snow
                    new UnityEngine.Color(0.75f,0.75f,0.75f), // Road
                    new UnityEngine.Color(0.5f,0.5f,0.5f), // City
                    new UnityEngine.Color(0.0f,0.0f,0.0f), // PointOfInterest
                    new UnityEngine.Color(0.1f,0.1f,0.1f), // Dungeon
                    new UnityEngine.Color(0.4f,0.2f,0.0f), // Cave
                    new UnityEngine.Color(0.85f,0.85f,0.85f), // Tower
                    new UnityEngine.Color(0.7f,0.2f,0), // Lava
                };
            }
            return _biomeColors;
        }

        public override async Task<NewCrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData genData)
        {
            IRandom rand = new MyRandom(genData.World.IdKey * 13 + genData.World.MaxMapId * 131);
            int width = 16 * MathUtils.IntRange(6, 10, rand);
            int height = 16 * MathUtils.IntRange(4, 6, rand);

            CrawlerMap map = genData.World.CreateMap(genData, width, height);
            map.DungeonArt = _gameData.Get<DungeonArtSettings>(null).Get(map.DungeonArtId);


            byte[,] overrides = new byte[map.Width, map.Height];
            long[,] terrain = new long[map.Width, map.Height];
            long[,] regionCells = new long[map.Width, map.Height];
            Reward[,] objects = new Reward[map.Width, map.Height];

            List<ZoneRegion> regions = new List<ZoneRegion>();

            List<ZoneType> allZoneTypes = _gameData.Get<ZoneTypeSettings>(null).GetData().OrderBy(x => x.MinLevel).ToList();

            List<long> okZoneIds = allZoneTypes.Where(x => x.GenChance > 0).Select(x => x.IdKey).ToList();

            List<UnityEngine.Color> biomeColors = GetZoneColors();

            int startMapEdgeSize = 4;

            int cityDistanceFromEdge = startMapEdgeSize * 2;

            int fullRegionZones = allZoneTypes.Where(x => x.MinLevel <= 100 && x.GenChance > 0).Count();

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

            points = points.OrderBy(p => p.X * sortx).ThenBy(p => p.Y * sorty).ToList();

            List<MyPoint2> origPoints = new List<MyPoint2>(points);

            foreach (MyPoint2 myPoint in origPoints)
            {
                myPoint.X = (int)(myPoint.X);
                myPoint.Y = (int)(myPoint.Y);
            }

            MyPoint2 firstPoint = points[0];

            points = points.OrderBy(p =>
                Math.Sqrt(
                    (p.X - firstPoint.X) * (p.X - firstPoint.X) +
                    (p.Y - firstPoint.Y) * (p.Y - firstPoint.Y)
                    )).ToList();


            origPoints = new List<MyPoint2>(points);

            int level = 1;
            int levelDelta = 7;
            float spreadDelta = 0.2f;
            float dirDelta = 0.3f;

            long cityZoneId = 0;
            long waterZoneId = allZoneTypes.FirstOrDefault(x => x.Name == "Water").IdKey;
            long roadZoneId = allZoneTypes.FirstOrDefault(x => x.Name == "Road").IdKey;
            long poiZoneId = allZoneTypes.FirstOrDefault(x => x.Name == "PointOfInterest").IdKey;
            long mountainZoneId = allZoneTypes.FirstOrDefault(x => x.Name == "Mountains").IdKey;

            List<ZoneType> startOkZones = allZoneTypes.Where(x => x.GenChance > 0 && x.IdKey != ZoneTypes.Mountains).ToList();
            while (points.Count > 0 && startOkZones.Count > 0)
            {
                List<ZoneType> okZones = startOkZones.Where(x => x.MinLevel <= level).ToList();

                if (okZones.Count < 1)
                {
                    break;
                }

                MyPoint2 centerPoint = points[0];

                points.Remove(centerPoint);

                ZoneType biomeType = okZones[rand.Next() % okZones.Count];

                startOkZones.Remove(biomeType);

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
                return new NewCrawlerMap() { Map = map };
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
                        if (terrain[x, y] == 0)
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

                map.Regions = regions;

                foreach (ZoneRegion region in regions)
                {
                    region.Name = _zoneGenService.GenerateZoneName(region.ZoneTypeId, rand.Next(), false);
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

                            if (terrain[x, y] != 0)
                            {
                                continue;
                            }

                            float xpct = (x - xcenter) / xrad;
                            float ypct = (y - ycenter) / yrad;

                            float distScale = Mathf.Sqrt(xpct * xpct + ypct * ypct);

                            if (distScale <= 1)
                            {
                                terrain[x, y] = region.ZoneTypeId;
                                regionCells[x, y] = region.ZoneTypeId;
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
                        else if (y >= map.Height - maxCheckRadius - 1)
                        {
                            cornerIndex = 1;
                            ycorner = map.Height - 1;
                        }
                    }
                    else if (x >= map.Width - maxCheckRadius - 1)
                    {
                        xcorner = map.Width - 1;
                        if (y <= maxCheckRadius)
                        {
                            cornerIndex = 2;
                            ycorner = 0;
                        }
                        else if (y >= map.Height - maxCheckRadius - 1)
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
                        int currRadius = (int)cornerRadii[cornerIndex] + startMapEdgeSize;


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

                        if (cx < map.Width / 2 && x > cx)
                        {
                            continue;
                        }

                        if (cx > map.Width / 2 && x < cx)
                        {
                            continue;
                        }

                        if (cy < map.Height / 2 && y > cy)
                        {
                            continue;
                        }

                        if (cy > map.Height / 2 && y < cy)
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
            // Roads between cities

            List<MyPoint2> remainingPoints = new List<MyPoint2>(origPoints);
            remainingPoints = remainingPoints.OrderBy(p => p.X).ToList();

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
                    if (terrain[x, ny] == roadZoneId)
                    {
                        foundRoad = true;
                        for (int xx = nx; xx > x; xx--)
                        {
                            if (terrain[xx, ny] == roadZoneId)
                            {
                                break;
                            }
                            terrain[xx, ny] = roadZoneId;

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
                int midy = (int)(cy + (int)MathUtils.FloatRange(0.33f, 0.67f, rand) * dy);

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
                        for (int xx = Math.Min(sx, ex); xx <= Math.Max(sx, ex); xx++)
                        {
                            terrain[xx, sy] = roadZoneId;
                        }
                        for (int yy = Math.Min(sy, ey); yy <= Math.Max(sy, ey); yy++)
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

            // Mountains at zone borders. (okZoneIds if  two diff make a small blob...only replacing things in ok biomeIds

            int crad = 1;
            int rrad = 2;
            int trad = Math.Max(crad, rrad);
            for (int x = trad; x < map.Width - trad; x++)
            {
                for (int y = trad; y < map.Height - trad; y++)
                {
                    List<long> currOkZoneIds = new List<long>();
                    bool nearRoad = false;

                    // Check for roads.
                    for (int xx = x - rrad; xx <= x + rrad; xx++)
                    {
                        for (int yy = y - rrad; yy <= y + rrad; yy++)
                        {
                            if (terrain[xx, yy] == roadZoneId)
                            {
                                nearRoad = true;
                                break;
                            }
                        }
                    }

                    if (nearRoad)
                    {
                        continue;
                    }

                    // Now check smaller radius for diff biomes.
                    for (int xx = x - crad; xx <= x + crad; xx++)
                    {
                        for (int yy = y - crad; yy <= y + crad; yy++)
                        {
                            long tid = regionCells[xx, yy];
                            if (tid != mountainZoneId && okZoneIds.Contains(tid))
                            {
                                if (!currOkZoneIds.Contains(tid))
                                {
                                    currOkZoneIds.Add(tid);
                                }
                            }
                        }
                    }

                    int nrad = rand.NextDouble() < 0.2f ? 1 : 0;

                    if (currOkZoneIds.Count > 1)
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
                    map.Set(x, y, CellIndex.Terrain, (short)(terrain[x, y]));
                    map.Set(x, y, CellIndex.Region, (short)regionCells[x, y]);
                }
            }

            for (int c = 0; c < origPoints.Count; c++)
            {
                MyPoint2 pt = origPoints[c];

                int cityLevel = 1;
                ZoneRegion zoneRegion = regions.FirstOrDefault(x => x.CenterX == (int)pt.X && x.CenterY == (int)pt.Y);

                if (zoneRegion != null)
                {
                    cityLevel = (int)zoneRegion.Level;
                }

                terrain[(int)pt.X, (int)pt.Y] = cityZoneId;
                CrawlerMapGenData cityGenData = new CrawlerMapGenData()
                {
                    World = genData.World,
                    MapType = CrawlerMapTypes.City,
                    Level = cityLevel,
                    FromMapId = map.IdKey,
                    FromMapX = (int)(pt.X),
                    FromMapZ = (int)(pt.Y),
                };


                map.Set((int)(pt.X), (int)pt.Y, CellIndex.Building, BuildingTypes.City);

                int xx = (int)pt.X;
                int yy = (int)pt.Y;

                int dx = 0;
                int dy = 0;

                if (map.Get(xx, yy + 1, CellIndex.Terrain) == ZoneTypes.Road)
                {
                    dx = 0;
                    dy = 1;
                }
                else if (map.Get(xx, yy - 1, CellIndex.Terrain) == ZoneTypes.Road)
                {
                    dx = 0;
                    dy = -1;
                }
                else if (map.Get(xx - 1, yy, CellIndex.Terrain) == ZoneTypes.Road)
                {
                    dx = -1;
                    dy = 0;
                }
                else if (map.Get(xx + 1, yy, CellIndex.Terrain) == ZoneTypes.Road)
                {
                    dx = 1;
                    dy = 0;
                }

                int dirAngle = DirUtils.DirDeltaToAngle(dx, dy);

                map.Set(xx, yy, CellIndex.Dir, dirAngle / CrawlerMapConstants.DirToAngleMult);

                CrawlerMap cityMap = await _mapGenService.Generate(party, world, cityGenData);

                cityMap.FromPlaceName = map.GetName(xx, yy);

                cityLevel += levelDelta;
            }

            // Add random dungeons and stuff on the map
            samplingData = new SamplingData()
            {
                Count = map.Width * map.Height / 80,
                MaxAttemptsPerItem = 20,
                XMin = cityDistanceFromEdge,
                XMax = map.Width - cityDistanceFromEdge,
                YMin = cityDistanceFromEdge,
                YMax = map.Height - cityDistanceFromEdge,
                MinSeparation = 10,
                Seed = rand.Next(),
            };

            List<MyPoint2> dungeonPoints = _samplingService.PlanePoissonSample(samplingData);

            double minDistFromCity = 8;

            int dungeonAttempts = dungeonPoints.Count;
            int dungeonSuccess = 0;
            foreach (MyPoint2 p in dungeonPoints)
            {
                int xx = (int)p.X;
                int yy = (int)p.Y;

                if (!okZoneIds.Contains(map.Get(xx, yy, CellIndex.Terrain)))
                {
                    continue;
                }

                bool tooCloseToCity = false;
                foreach (ZoneRegion region in map.Regions)
                {
                    double ddx = region.CenterX - xx;
                    double ddy = region.CenterY - yy;

                    if (Math.Sqrt(ddx * ddx + ddy * ddy) < minDistFromCity)
                    {
                        tooCloseToCity = true;
                        break;
                    }
                }

                if (tooCloseToCity)
                {
                    continue;
                }

                long dungeonLevel = 2 + await _worldService.GetMapLevelAtPoint(world, map.IdKey, xx, yy) * 5 / 4;
                CrawlerMapGenData dungeonGenData = new CrawlerMapGenData()
                {
                    World = genData.World,
                    MapType = CrawlerMapTypes.Dungeon,
                    Level = (int)dungeonLevel,
                    FromMapId = map.IdKey,
                    FromMapX = (int)(xx),
                    FromMapZ = (int)(yy),
                };

                dungeonSuccess++;
                CrawlerMap dungeonMap = await _mapGenService.Generate(party, world, dungeonGenData);

                long buildingTypeId = GetBuildingIdFromZoneTypeId(dungeonMap.ZoneTypeId);
                map.Set(xx, yy, CellIndex.Building, buildingTypeId);
            }

            List<Riddle> riddles = _gameData.Get<RiddleSettings>(_gs.ch).GetData().ToList();

            CrawlerMapSettings mapSettings = _gameData.Get<CrawlerMapSettings>(_gs.ch);

            CrawlerMapType dungeonType = mapSettings.Get(CrawlerMapTypes.Dungeon);

            List<CrawlerMap> dungeonMaps = world.Maps.Where(x => x.CrawlerMapTypeId == CrawlerMapTypes.Dungeon && x.MapFloor == 1).OrderBy(x => x.Level).ToList();


            List<List<CrawlerMap>> dungeonMapGroups = new List<List<CrawlerMap>>();

            for (int d = 0; d < dungeonMaps.Count; d++)
            {
                CrawlerMap dmap = dungeonMaps[d];

                List<CrawlerMap> otherDungeonMaps = world.Maps.Where(x => x.CrawlerMapTypeId == CrawlerMapTypes.Dungeon &&
                x.Name == dmap.Name && x.IdKey >= dmap.IdKey && x.IdKey <= dmap.IdKey + 6).OrderBy(x => x.MapFloor).ToList();

                dungeonMapGroups.Add(otherDungeonMaps);

            }

            for (int d = 0; d < dungeonMapGroups.Count; d++)
            {
                List<CrawlerMap> floors = dungeonMapGroups[d];

                List<long> floorIds = floors.Select(x => x.IdKey).ToList();

                CrawlerMap entranceMap = floors.First();

                MapCellDetail exitDetail = entranceMap.Details.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Map &&
                !floorIds.Contains(x.EntityId));

                if (exitDetail != null)
                {
                    CrawlerMap exitMap = world.GetMap(exitDetail.EntityId);
                    if (exitMap != null)
                    {
                        entranceMap.FromPlaceName = exitMap.GetName(exitDetail.ToX, exitDetail.ToZ);
                    }
                }
            }

            for (int dungeonIndex = 0; dungeonIndex < dungeonMapGroups.Count; dungeonIndex++)
            {
                List<CrawlerMap> floors = dungeonMapGroups[dungeonIndex];

                List<long> floorIds = floors.Select(x => x.IdKey).ToList();

                CrawlerMap entranceMap = floors.First();

                if ((dungeonIndex > 4 && rand.NextDouble() < dungeonType.QuestItemEntranceUnlockChance) || dungeonIndex == dungeonMapGroups.Count - 1)
                {

                    string questItemName = _lootGenService.GenerateItemNames(rand, 1).First();

                    int lookbackDistance = Math.Min(dungeonIndex - 1, 4 + dungeonIndex / 3);

                    List<int> okIndexes = new List<int>();

                    for (int i = dungeonIndex - 1; i >= 0 && dungeonIndex - i <= lookbackDistance + 1; i--)
                    {
                        okIndexes.Add(i);
                    }

                    int chosenIndex = okIndexes[rand.Next() % okIndexes.Count];

                    List<CrawlerMap> targetMaps = dungeonMapGroups[chosenIndex];

                    List<MapCellDetail> openQuestDetails = new List<MapCellDetail>();

                    foreach (CrawlerMap cmap in targetMaps)
                    {
                        openQuestDetails.AddRange(cmap.Details.Where(x => x.EntityTypeId == EntityTypes.QuestItem && x.EntityId == 0));
                    }

                    if (openQuestDetails.Count > 0)
                    {
                        MapCellDetail chosenDetail = openQuestDetails[rand.Next() % openQuestDetails.Count];

                        long nextQuestItemId = 1;
                        if (world.QuestItems.Count > 0)
                        {
                            nextQuestItemId = world.QuestItems.Max(x => x.IdKey) + 1;
                        }
                        world.QuestItems.Add(new WorldQuestItem()
                        {
                            IdKey = nextQuestItemId,
                            Name = questItemName,
                            FoundInMapId = targetMaps[0].IdKey,
                            UnlocksMapId = entranceMap.IdKey,
                        });

                        entranceMap.MapQuestItemId = nextQuestItemId;

                    }
                }
                else if (dungeonIndex > 2 && dungeonIndex < dungeonMapGroups.Count - 1 && rand.NextDouble() <= dungeonType.RiddleUnlockChance && riddles.Count > 0)
                {
                    Riddle riddle = riddles[rand.Next() % riddles.Count];

                    long minFloor = floors.Min(x => x.MapFloor);
                    long maxFloor = floors.Max(x => x.MapFloor);

                    if (maxFloor < 2)
                    {
                        continue;
                    }

                    long floorChosen = MathUtils.LongRange(minFloor + 1, maxFloor, rand);

                    floorChosen = 2;

                    CrawlerMap lockedFloor = floors.FirstOrDefault(x=>x.MapFloor == floorChosen);

                    if (lockedFloor == null)
                    {
                        continue;
                    }

                    CrawlerMap prevFloor = floors.FirstOrDefault(x => x.MapFloor == floorChosen - 1);

                    if (prevFloor == null)
                    {
                        continue;
                    }

                    string[] lines = riddle.Desc.Split('\n');

                    if (lines.Length < 2)
                    {
                        continue;
                    }

                    List<MyPoint2> openPoints = new List<MyPoint2>();

                    for (int x =0; x < prevFloor.Width; x++)
                    {
                        for (int z = 0; z < prevFloor.Height; z++)
                        {
                            if (prevFloor.Get(x,z,CellIndex.Terrain) < 1 ||
                               prevFloor.Get(x,z,CellIndex.Encounter) > 0 ||
                                prevFloor.Get(x,z,CellIndex.Magic) > 0 ||
                                prevFloor.Get(x,z,CellIndex.Disables) > 0)
                            {
                                continue;
                            }

                            MapCellDetail detail = prevFloor.Details.FirstOrDefault(d => d.X == x && d.Z == z);

                            if (detail != null)
                            {
                                continue;
                            }

                            openPoints.Add(new MyPoint2(x,z));
                        }
                    }

                    if (openPoints.Count < lines.Length)
                    {
                        continue;
                    }

                    riddles.Remove(riddle);

                    lockedFloor.RiddleId = riddle.IdKey;

                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (!String.IsNullOrEmpty(lines[i]))
                        {
                            MyPoint2 openPoint = openPoints[rand.Next() % openPoints.Count];
                            prevFloor.Details.Add(new MapCellDetail()
                            {
                                EntityTypeId = EntityTypes.Riddle,
                                EntityId = riddle.IdKey,
                                X = (int)openPoint.X,
                                Z = (int)openPoint.Y,
                                Index = i
                            });
                        }
                    }
                }
            }

            // Now remove all empty quest item detail slots.

            foreach (CrawlerMap map2 in world.Maps)
            {
                map2.Details = map2.Details.Where(x => x.EntityTypeId != EntityTypes.QuestItem ||
                x.EntityId != 0).ToList();
            }

            // Log quest items.

            foreach (CrawlerMap cmap in world.Maps)
            {
                if (cmap.MapQuestItemId > 0)
                {
                    WorldQuestItem wqi = world.QuestItems.FirstOrDefault(x => x.IdKey == cmap.MapQuestItemId);
                    if (wqi != null)
                    {
                        CrawlerMap otherMap = world.GetMap(wqi.FoundInMapId);

                        _logService.Info("Map "
                            + cmap.Name
                            + " Requires Item "
                            + wqi.Name
                            + " Found in " + otherMap.Name);
                    }
                }
            }
                
            // Now random trees. (1000 + building Id vs building id)

            IReadOnlyList<TreeType> treeTypes = _gameData.Get<TreeTypeSettings>(null).GetData();

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    if (map.Get(x, z, CellIndex.Building) > 0)
                    {
                        continue;
                    }
                    ZoneType ztype = allZoneTypes.FirstOrDefault(t => t.IdKey == map.Get(x, z, CellIndex.Terrain));
                    if (ztype != null && ztype.TreeTypes != null && ztype.TreeTypes.Count > 0)
                    {
                        double chance = ztype.TreeDensity * CrawlerMapConstants.TreeChanceScale;

                        if (rand.NextDouble() < chance)
                        {
                            long treeTypeId = 0;
                            for (int tries = 0; tries < 20; tries++)
                            {
                                ZoneTreeType zttype = ztype.TreeTypes[rand.Next() % ztype.TreeTypes.Count];
                                TreeType ttype = treeTypes.FirstOrDefault(x => x.IdKey == zttype.TreeTypeId);
                                if (!ttype.HasFlag(TreeFlags.IsBush))
                                {
                                    treeTypeId = ttype.IdKey;
                                    break;
                                }
                            }

                            if (treeTypeId > 0)
                            {
                                map.Set(x, z, CellIndex.Tree, treeTypeId);
                            }
                        }
                    }
                }
            }

            NewCrawlerMap newMap = new NewCrawlerMap()
            {
                Map = map,
                EnterX = -1,
                EnterZ = -1,
            };
            map.Name = "The World";

            return newMap;
        }
    }
}
