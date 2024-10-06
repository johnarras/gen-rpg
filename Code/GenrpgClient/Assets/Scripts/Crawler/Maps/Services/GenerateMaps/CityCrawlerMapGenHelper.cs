
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Zones.Settings;
using UnityEngine;
using System.Threading.Tasks;
using Genrpg.Shared.Crawler.Maps.Settings;

namespace Assets.Scripts.Crawler.Maps.Services.GenerateMaps
{
    public class CityCrawlerMapGenHelper : BaseCrawlerMapGenHelper
    {

        public override long GetKey() { return CrawlerMapTypes.City; }

        public override async Task<NewCrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData genData)
        {
            await Task.CompletedTask;
            MyRandom rand = new MyRandom(genData.World.IdKey*3 + genData.World.MaxMapId*17);

            IReadOnlyList<ZoneType> allZoneTypes = _gameData.Get<ZoneTypeSettings>(null).GetData();

            long cityZoneTypeId = allZoneTypes.FirstOrDefault(x => x.Name == "City")?.IdKey ?? 1;
            long roadZoneTypeId = allZoneTypes.FirstOrDefault(x => x.Name == "Road")?.IdKey ?? 1;

            CrawlerMapType mapType = _gameData.Get<CrawlerMapSettings>(_gs.ch).Get(CrawlerMapTypes.City);


            int width = (int)MathUtils.LongRange(mapType.MinWidth, mapType.MaxWidth, rand);
            int height = (int)MathUtils.LongRange(mapType.MinHeight, mapType.MaxHeight, rand);
            genData.ZoneTypeId = cityZoneTypeId;
            CrawlerMap map = genData.World.CreateMap(genData, width, height);

            map.Name = _zoneGenService.GenerateZoneName(genData.ZoneTypeId, rand.Next(), true);
            bool[,] clearCells = AddCorridors(map, genData, rand, 1.0f);

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


            IReadOnlyList<BuildingType> buildings = _gameData.Get<BuildingSettings>(null).GetData();

            List<BuildingType> crawlerBuildings = buildings.Where(x => x.IsCrawlerBuilding).ToList();

            List<BuildingType> fillerBuildings = crawlerBuildings.Where(x => x.VariationCount > 1).ToList();

            List<BuildingType> requiredBuildings = crawlerBuildings.Where(x => x.VariationCount <= 1).ToList();

            List<MyPoint> points = new List<MyPoint>();

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

                            if (sx < 0 || sx >= map.Width)
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

                            int dirAngle = DirUtils.DirDeltaToAngle(okDir.X, okDir.Y);

                            map.Set(xx, yy, CellIndex.Dir, dirAngle/CrawlerMapConstants.DirToAngleMult);

                            BuildingType btype = fillerBuildings[rand.Next() % fillerBuildings.Count];

                            map.Set(xx, yy, CellIndex.Building, btype.IdKey);

                            points.Add(new MyPoint(xx, yy));
                        }

                    }
                }
            }


            points = points.Where(x => x.X > 0 && x.Y > 0 && x.X < map.Width - 1 && x.Y < map.Height - 1).ToList();
            foreach (BuildingType btype in requiredBuildings)
            {
                if (points.Count < 1)
                {
                    break;
                }

                MyPoint currPoint = points[rand.Next() % points.Count];
                points.Remove(currPoint);
                
                map.Set((int)currPoint.X, (int)currPoint.Y, CellIndex.Building, btype.IdKey);
            }

            List<long> randomMapZoneIds = new List<long>(_newMapZoneIds);

            int dungeonLevel = map.Level + 1;
            while (randomMapZoneIds.Count > 0)
            {
                if (points.Count < 1)
                {
                    continue;
                }

                long newZoneTypeId = randomMapZoneIds[rand.Next() % randomMapZoneIds.Count];

                randomMapZoneIds.Remove(newZoneTypeId);

                MyPoint currPoint = points[rand.Next() % points.Count];
                points.Remove(currPoint);

                CrawlerMapGenData dungeonGenData = new CrawlerMapGenData()
                {
                    World = genData.World,
                    MapType = CrawlerMapTypes.Dungeon,
                    ZoneTypeId = newZoneTypeId,
                    Level = dungeonLevel++,
                    FromMapId = map.IdKey,
                    FromMapX = (int)(currPoint.X),
                    FromMapZ = (int)(currPoint.Y),
                };

                map.Set((int)currPoint.X, (int)currPoint.Y, CellIndex.Building, GetBuildingIdFromZoneTypeId(newZoneTypeId));
                CrawlerMap dungeonMap = await _mapGenService.Generate(party, world, dungeonGenData);

                if (rand.NextDouble() < 0.6)
                {
                    break;
                }
            }

            IReadOnlyList<ZoneType> zoneTypes = _gameData.Get<ZoneTypeSettings>(null).GetData();

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {

                    if (map.Get(x, y, CellIndex.Building) != 0)
                    {
                        map.Set(x, y, CellIndex.Terrain, cityZoneTypeId);
                    }
                    else if (clearCells[x,y])
                    {
                        map.Set(x, y, CellIndex.Terrain, roadZoneTypeId);
                    }
                    else
                    {

                        map.Set(x, y, CellIndex.Terrain, 1);
                    }
                }
            }
            return new NewCrawlerMap() { Map = map, EnterX = gateX, EnterZ = gateZ };   
        }
    }
}
