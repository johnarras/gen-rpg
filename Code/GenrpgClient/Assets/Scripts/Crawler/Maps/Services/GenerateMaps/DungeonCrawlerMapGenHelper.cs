using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services.GenerateMaps
{
    public class DungeonCrawlerMapGenHelper : BaseCrawlerMapGenHelper
    {
        public override ECrawlerMapTypes GetKey() { return ECrawlerMapTypes.Dungeon; }

        public override async Awaitable<NewCrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData genData)
        {
            await Task.CompletedTask;
            MyRandom rand = new MyRandom(genData.World.IdKey * 5 + genData.World.MaxMapId * 19 + genData.CurrFloor);

            CrawlerMap map = null;
            if (genData.MaxFloor == 0 || genData.PrevMap == null)
            {
                genData.MaxFloor = MathUtils.IntRange(2, 4, rand);
                genData.CurrFloor = 1;
                if (rand.NextDouble() < 0.2f)
                {
                    genData.MaxFloor++;
                }

                int minsize = 15;
                int maxsize = 20;
                int width = MathUtils.IntRange(minsize, maxsize, rand);
                int height = MathUtils.IntRange(minsize, maxsize, rand);
                genData.Looping = true;
                genData.SimpleDungeon = true;

                if (rand.NextDouble() < 0.5f)
                {
                    genData.SimpleDungeon = false;
                    width = MathUtils.IntRange(minsize * 3 / 2, maxsize * 3 / 2, rand);
                    height = MathUtils.IntRange(minsize * 3 / 2, maxsize * 3 / 2, rand);
                    genData.Looping = false;
                }
                if (genData.ZoneTypeId == ZoneTypes.Tower)
                {
                    genData.Looping = false;
                    width = Math.Max(10, width / 2);
                    height = Math.Max(10, height / 2);
                    genData.SimpleDungeon = true;
                }


                if (genData.ZoneTypeId == 0)
                {
                    genData.ZoneTypeId = _newMapZoneIds[rand.Next() % _newMapZoneIds.Length];
                }

                map = genData.World.CreateMap(genData, width, height);
                genData.Name = _zoneGenService.GenerateZoneName(genData.ZoneTypeId, rand.Next(), false);

            }
            else
            {
                genData.Level++;
                genData.CurrFloor++;
                map = genData.World.CreateMap(genData, genData.PrevMap.Width, genData.PrevMap.Height);
            }

            genData.PrevMap = map;

            map.Name = genData.Name;

            int exitX = -1;
            int exitZ = -1;
            int enterX = -1;
            int enterZ = -1;

            List<PointXZ> questItemLocations = new List<PointXZ>();

            if (genData.SimpleDungeon)
            {
                float chance = 0.6f;
                for (int x = 0; x < map.Width; x++)
                {
                    for (int z = 0; z < map.Height; z++)
                    {
                        map.Set(x, z, CellIndex.Terrain, genData.ZoneTypeId);
                        int index = map.GetIndex(x, z);
                        int wallValue = 0;
                        if (rand.NextDouble() < chance)
                        {
                            if (x == map.Width - 1 && !map.Looping)
                            {
                                wallValue |= WallTypes.Wall << MapWallBits.EWallStart;
                            }
                            else if (rand.NextDouble() < chance)
                            {
                                int value = MathUtils.IntRange(0, WallTypes.Max - 1, rand);
                                wallValue |= value << MapWallBits.EWallStart;
                            }

                            if (z == map.Height - 1 && !map.Looping)
                            {
                                wallValue |= WallTypes.Wall << MapWallBits.NWallStart;
                            }
                            else if (rand.NextDouble() < chance)
                            {
                                int value = MathUtils.IntRange(0, WallTypes.Max - 1, rand);
                                wallValue |= (value << MapWallBits.NWallStart);
                            }

                        }
                        else
                        {
                            if (x == map.Width - 1 && !map.Looping)
                            {
                                wallValue |= WallTypes.Wall << MapWallBits.EWallStart;
                            }
                            if (z == map.Height - 1 && !map.Looping)
                            {
                                wallValue |= WallTypes.Wall << MapWallBits.NWallStart;
                            }
                        }
                        map.Set(x, z, CellIndex.Walls, wallValue);
                    }
                }

                enterX = rand.Next() % map.Width;
                enterZ = rand.Next() % map.Height;

                map.AddBits(enterX, enterZ, CellIndex.Encounter, MapEncounters.OtherFeature);
                do
                {
                    exitX = rand.Next() % map.Width;
                    exitZ = rand.Next() % map.Height;
                }
                while (enterX == exitX && enterZ == exitZ);

                map.AddBits(exitX, exitZ, CellIndex.Encounter, MapEncounters.OtherFeature);
                List<PointXZ> usedPoints = new List<PointXZ>();
                usedPoints.Add(new PointXZ(enterX, enterZ));
                usedPoints.Add(new PointXZ(exitX, exitZ));

                for (int i = 0; i < 3; i++)
                {
                    do
                    {
                        int px = rand.Next() % map.Width;
                        int pz = rand.Next() % map.Height;

                        bool matchedExistingPoint = false;

                        foreach (PointXZ pt in usedPoints)
                        {
                            if (pt.X == px && pt.Z == pz)
                            {
                                matchedExistingPoint = true;
                                break;
                            }
                        }

                        if (!matchedExistingPoint)
                        {
                            usedPoints.Add(new PointXZ(px, pz));
                            questItemLocations.Add(new PointXZ(px, pz));
                            map.AddBits(px, pz, CellIndex.Encounter, MapEncounters.OtherFeature);
                            break;
                        }
                    }
                    while (true);

                }

                int encountersToPlace = (int)(map.Width * map.Height * EncounterChance);

                int encounterTries = encountersToPlace * 20;

                for (int i = 0; i < encounterTries && encountersToPlace > 0; i++)
                {
                    int tx = rand.Next() % map.Width;
                    int tz = rand.Next() % map.Height;

                    if (map.Get(tx, tz, CellIndex.Encounter) != 0)
                    {
                        continue;
                    }
                    else
                    {
                        encountersToPlace--;
                        map.Set(tx, tz, CellIndex.Encounter, GetRandomEncounter(rand));
                    }
                }
            }
            else
            {
                bool[,] clearCells = AddCorridors(map, genData, rand, 0.66f);

                // Add rooms first.
                int roomEdgeDist = 5;
                List<PointXZ> okPoints = new List<PointXZ>();
                for (int x = roomEdgeDist; x < map.Width - roomEdgeDist; x++)
                {
                    for (int y = roomEdgeDist; y < map.Height - roomEdgeDist; y++)
                    {
                        if (clearCells[x, y])
                        {
                            okPoints.Add(new PointXZ() { X = x, Z = y });
                        }
                    }
                }

                int roomCount = (int)(Math.Sqrt(map.Width * map.Height) * 0.3f);

                List<PointXZ> roomCenters = new List<PointXZ>();

                for (int r = 0; r < roomCount; r++)
                {
                    PointXZ roomCenter = okPoints[rand.Next() % okPoints.Count];
                    okPoints.Remove(roomCenter);
                    roomCenters.Add(roomCenter);
                    int sx = roomCenter.X - GetRoomDeltaSize(rand);
                    int ex = roomCenter.X + GetRoomDeltaSize(rand);
                    int sz = roomCenter.Z - GetRoomDeltaSize(rand);
                    int ez = roomCenter.Z + GetRoomDeltaSize(rand);

                    for (int x = sx; x < ex; x++)
                    {
                        for (int z = sz; z < ez; z++)
                        {
                            clearCells[x, z] = true;
                            map.AddBits(x, z, CellIndex.Walls, 1 << MapWallBits.IsRoomBitOffset);
                        }
                    }
                }

                for (int x = 0; x < map.Width; x++)
                {
                    for (int z = 0; z < map.Height; z++)
                    {

                        if (clearCells[x, z])
                        {
                            map.Set(x, z, CellIndex.Terrain, genData.ZoneTypeId);
                        }

                        int wallValue = 0;
                        int leftx = (x + map.Width - 1) % map.Width;
                        int rightx = (x + 1) % map.Width;
                        int upz = (z + 1) % map.Height;
                        int downz = (z + map.Height - 1) % map.Height;

                        if (clearCells[x, z])
                        {
                            wallValue = map.Get(x, z, CellIndex.Walls);
                            if (!clearCells[rightx, z])
                            {
                                wallValue |= (WallTypes.Wall << MapWallBits.EWallStart);
                            }
                            if (!clearCells[x, upz])
                            {
                                wallValue |= (WallTypes.Wall << MapWallBits.NWallStart);
                            }
                            map.AddBits(x, z, CellIndex.Walls, wallValue);

                            if (!clearCells[leftx, z])
                            {
                                byte currWallValue = map.Get(leftx, z, CellIndex.Walls);
                                currWallValue |= (WallTypes.Wall << MapWallBits.EWallStart);
                                map.AddBits(leftx, z, CellIndex.Walls, currWallValue);
                            }
                            if (!clearCells[x, downz])
                            {
                                byte currWallValue = map.Get(x, downz, CellIndex.Walls);
                                currWallValue |= (WallTypes.Wall << MapWallBits.NWallStart);
                                map.AddBits(x, downz, CellIndex.Walls, currWallValue);
                            }
                        }
                    }
                }
                PointXZ entrancePoint = roomCenters[rand.Next() % roomCenters.Count];
                enterX = entrancePoint.X;
                enterZ = entrancePoint.Z;
                roomCenters.Remove(entrancePoint);
                PointXZ exitPoint = roomCenters[rand.Next() % roomCenters.Count];
                exitX = exitPoint.X;
                exitZ = exitPoint.Z;

                for (int i = 0; i < 3; i++)
                {
                    if (okPoints.Count > 0)
                    {
                        PointXZ pt = okPoints[rand.Next() % okPoints.Count];
                        okPoints.Remove(pt);
                        questItemLocations.Add(pt);
                    }
                }

                int encountersToPlace = (int)(okPoints.Count * EncounterChance);

                int encounterTries = encountersToPlace * 20;

                for (int i = 0; i < encounterTries && encountersToPlace > 0; i++)
                {
                    if (okPoints.Count < 1)
                    {
                        continue;
                    }

                    PointXZ pt = okPoints[rand.Next() % okPoints.Count];
                    okPoints.Remove(pt);

                    if (map.Get(pt.X, pt.Z, CellIndex.Encounter) != 0)
                    {
                        continue;
                    }
                    else
                    {
                        map.Set(pt.X, pt.Z, CellIndex.Encounter, GetRandomEncounter(rand));
                        encountersToPlace--;
                    }
                }
            }

            foreach (PointXZ pt in questItemLocations)
            {
                map.Details.Add(new MapCellDetail() { EntityTypeId = EntityTypes.QuestItem, X = pt.X, Z = pt.Z });
            }

            if (genData.CurrFloor < genData.MaxFloor)
            {
                long currMapId = genData.FromMapId;
                int currFromX = genData.FromMapX;
                int currFromZ = genData.FromMapZ;

                genData.FromMapId = map.IdKey;
                genData.FromMapX = exitX;
                genData.FromMapZ = exitZ;

                await _mapGenService.Generate(party, world, genData);

                genData.FromMapId = currMapId;
                genData.FromMapX = currFromX;
                genData.FromMapZ = currFromZ;
            }

            return new NewCrawlerMap() { Map = map, EnterX = enterX, EnterZ = enterZ };
        }


        protected int GetRandomEncounter(IRandom rand)        
        {
            if (rand.NextDouble() < 0.2f)
            {
                return MapEncounters.Treasure;
            }
            else if (rand.NextDouble() < 0.3f)
            {
                return MapEncounters.Trap;
            }
            else
            {
                return MapEncounters.Monsters;
            }

        }
        const float extraLengthChance = 0.25f;
        protected int GetRoomDeltaSize(MyRandom rand)
        {
            int retval = 1;

            for (int i = 0; i < 3; i++)
            {
                if (rand.Next() < extraLengthChance)
                {
                    retval++;
                }
                else
                {
                    break;
                }
            }
            return retval;
        }
    }
}
