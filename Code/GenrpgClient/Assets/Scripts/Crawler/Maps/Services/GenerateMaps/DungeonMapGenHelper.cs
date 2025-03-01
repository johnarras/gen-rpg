﻿using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.MapGen.Entities;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;

namespace Assets.Scripts.Crawler.Maps.Services.GenerateMaps
{
    public class DungeonMapGenHelper : BaseCrawlerMapGenHelper
    {
        public override long GetKey() { return CrawlerMapTypes.Dungeon; }

        public override async Task<NewCrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData genData)
        {
            await Task.CompletedTask;
            IRandom rand = new MyRandom(genData.World.Seed / 3 + genData.World.MaxMapId * 19 + genData.CurrFloor);

            CrawlerMap map = null;

            CrawlerMapSettings mapSettings = _gameData.Get<CrawlerMapSettings>(_gs.ch);

            MapEncounterSettings encounterSettings = _gameData.Get<MapEncounterSettings>(_gs.ch);
            CrawlerMapGenType genType = genData.GenType;

            int roomEdgeDist = 3;

            if (genData.MaxFloor == 0 || genData.PrevMap == null)
            {
                genData.MaxFloor = MathUtils.IntRange(genType.MinFloors, genType.MaxFloors, rand);
                if (party.GameMode == ECrawlerGameModes.Roguelite)
                {
                    genData.MaxFloor = 1000;
                }
                if (genData.CurrFloor == 0)
                {
                    genData.CurrFloor = 1;
                }
                if (rand.NextDouble() < 0.2f && genType.MaxFloors > 1)
                {
                    genData.MaxFloor++;
                }

                genData.Looping = rand.NextDouble() < genType.LoopingChance ? true : false;
                genData.RandomWallsDungeon = rand.NextDouble() < genType.RandomWallsChance ? true : false;

                int width = MathUtils.IntRange(genType.MinWidth, genType.MaxWidth, rand);
                int height = MathUtils.IntRange(genType.MinHeight, genType.MaxHeight, rand);

                if (party.GameMode == ECrawlerGameModes.Roguelite)
                {
                    genData.RandomWallsDungeon = true;
                    width /= 2;
                    height /= 2;
                }
                if (!genData.RandomWallsDungeon)
                {
                    width = (int)(width * mapSettings.CorridorDungeonSizeScale);
                    height = (int)(height * mapSettings.CorridorDungeonSizeScale);  
                }

                map = _worldService.CreateMap(genData, (int)width, (int)height);
                genData.Name = _zoneGenService.GenerateZoneName(genData.ZoneType.IdKey, rand.Next(), false);


            }
            else
            {
                genData.Level++;
                genData.CurrFloor++;
                map = _worldService.CreateMap(genData, genData.PrevMap.Width, genData.PrevMap.Height);
            }

            genData.PrevMap = map;

            map.Name = genData.Name;

            int exitX = -1;
            int exitZ = -1;
            int enterX = -1;
            int enterZ = -1;

            List<PointXZ> questItemLocations = new List<PointXZ>();

            if (genData.RandomWallsDungeon)
            {
                double wallChance = MathUtils.FloatRange(genType.MinWallChance, genType.MaxWallChance, rand);
                double doorChance = MathUtils.FloatRange(genType.MinDoorChance, genType.MaxDoorChance, rand);
                for (int x = 0; x < map.Width; x++)
                {
                    for (int z = 0; z < map.Height; z++)
                    {
                        map.Set(x, z, CellIndex.Terrain, genData.ZoneType.IdKey);
                        int index = map.GetIndex(x, z);
                        int wallValue = 0;
                        if (rand.NextDouble() < wallChance)
                        {
                            if (x == map.Width - 1 && !map.Looping)
                            {
                                wallValue |= WallTypes.Wall << MapWallBits.EWallStart;
                            }
                            else if (rand.NextDouble() > doorChance)
                            {
                                wallValue |= WallTypes.Wall << MapWallBits.EWallStart;
                            }
                            else
                            {
                                wallValue |= WallTypes.Door << MapWallBits.EWallStart;
                            }

                            if (z == map.Height - 1 && !map.Looping)
                            {
                                wallValue |= WallTypes.Wall << MapWallBits.NWallStart;
                            }
                            else if (rand.NextDouble() > doorChance)
                            {
                                wallValue |= (WallTypes.Wall << MapWallBits.NWallStart);
                            }
                            else
                            {
                                wallValue |= WallTypes.Door << MapWallBits.NWallStart;
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

                ConnectOpenCells(map, genData, rand);

                
                double roomTimes = map.Width * map.Height / 200.0f;

                double roomremainder = roomTimes - (int)roomTimes;
                roomTimes = (int)roomTimes;
                if (rand.NextDouble() < roomremainder) 
                {
                    roomTimes++;
                }

                int maxRoomSize = 6;
                for (int r = 0; r < roomTimes; r++)
                {
                    int minx = MathUtils.IntRange(0, map.Width - maxRoomSize-1, rand);
                    int maxx = minx + MathUtils.IntRange(maxRoomSize / 2, maxRoomSize, rand);

                    int minz = MathUtils.IntRange(0, map.Height - maxRoomSize - 1, rand);
                    int maxz = MathUtils.IntRange(maxRoomSize / 2, maxRoomSize, rand);

                    for (int x = minx; x < maxx; x++)
                    {
                        for (int z = minz; z < maxz; z++)
                        {
                            map.Set(x, z, CellIndex.Walls, 0);
                        }
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
            }
            else
            {
                bool[,] clearCells = AddCorridors(map, genData, rand, MathUtils.FloatRange(genType.MinCorridorDensity,genType.MaxCorridorDensity,rand));

                // Add rooms first.
                List<PointXZ> openPoints = new List<PointXZ>();
                for (int x = roomEdgeDist; x < map.Width - roomEdgeDist; x++)
                {
                    for (int y = roomEdgeDist; y < map.Height - roomEdgeDist; y++)
                    {
                        if (clearCells[x, y])
                        {
                            openPoints.Add(new PointXZ() { X = x, Z = y });
                        }
                    }
                }

                int roomCount = (int)(Math.Sqrt(map.Width * map.Height) * 0.3f);

                List<PointXZ> roomCenters = new List<PointXZ>();

                for (int r = 0; r < roomCount; r++)
                {
                    if (openPoints.Count < 1)
                    {
                        break;
                    }
                    PointXZ roomCenter = openPoints[rand.Next() % openPoints.Count];
                    openPoints.Remove(roomCenter);
                    roomCenters.Add(roomCenter);
                    int sx = roomCenter.X - GetRoomDeltaSize(rand, roomEdgeDist);
                    int ex = roomCenter.X + GetRoomDeltaSize(rand, roomEdgeDist);
                    int sz = roomCenter.Z - GetRoomDeltaSize(rand, roomEdgeDist);
                    int ez = roomCenter.Z + GetRoomDeltaSize(rand, roomEdgeDist);

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
                            map.Set(x, z, CellIndex.Terrain, genData.ZoneType.IdKey);
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
                    if (openPoints.Count > 0)
                    {
                        PointXZ pt = openPoints[rand.Next() % openPoints.Count];
                        openPoints.Remove(pt);
                        questItemLocations.Add(pt);
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

                if (party.GameMode != ECrawlerGameModes.Roguelite)
                {
                    await _mapGenService.Generate(party, world, genData);
                }

                genData.FromMapId = currMapId;
                genData.FromMapX = currFromX;
                genData.FromMapZ = currFromZ;

                if (party.GameMode == ECrawlerGameModes.Roguelite)
                {
                    List<PointXZ> stairPoints = new List<PointXZ>();


                    for (int x = 0; x < map.Width; x++)
                    {
                        for (int z = 0; z < map.Height; z++)
                        {
                            if (map.Get(x,z,CellIndex.Terrain) == 0)
                            {
                                continue;
                            }
                            MapCellDetail currDetail = map.Details.FirstOrDefault(d => d.X == x && d.Z == z);
                            
                            if (currDetail != null)
                            {
                                continue;
                            }
                            stairPoints.Add(new PointXZ(x,z));

                        }
                    }

                    PointXZ downStairsPoint = stairPoints[rand.Next() % stairPoints.Count];

                    map.Details.Add(new MapCellDetail() { EntityTypeId = EntityTypes.Map, EntityId = map.IdKey+1, X = downStairsPoint.X, Z = downStairsPoint.Z });

                }


               

            }


            List<PointXZ> okPoints = new List<PointXZ>();

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    if (map.Get(x, z, CellIndex.Terrain) > 0 &&
                        map.Get(x, z, CellIndex.Building) == 0 &&
                        map.Get(x, z, CellIndex.Tree) == 0 &&
                        !map.Details.Any(d => d.X == x && d.Z == z))
                    {
                        okPoints.Add(new PointXZ(x, z));
                    }
                }
            }

            // add encounters.
            int encountersToPlace = (int)(okPoints.Count * encounterSettings.EncounterChance);

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

            IReadOnlyList<MapMagicType> mapMagics = _gameData.Get<MapMagicSettings>(_gs.ch).GetData();
            double specialTilechance = genData.GenType.SpecialTileChance;
            foreach (PointXZ pt in okPoints)
            {
                if (rand.NextDouble() > specialTilechance)
                {
                    continue;
                }

                foreach (MapMagicType mtype in mapMagics)
                {
                    if (map.Level < mtype.MinLevel || rand.NextDouble() > mtype.Weight)
                    {
                        continue;
                    }

                    int xmin = pt.X;
                    int xmax = pt.X;
                    int zmin = pt.Z;
                    int zmax = pt.Z;

                    while (xmin > 0 && rand.NextDouble() < mtype.SpreadChance)
                    {
                        xmin--;
                    }
                    while (xmax < map.Width-1 && rand.NextDouble() < mtype.SpreadChance)
                    {
                        xmax++;
                    }
                    while (zmin > 0 && rand.NextDouble() < mtype.SpreadChance)
                    {
                        zmin--;
                    }
                    while (zmax < map.Height-1 && rand.NextDouble() < mtype.SpreadChance)
                    {
                        zmax++;
                    }

                    for (int xx = xmin; xx <= xmax; xx++)
                    {
                        for (int zz = zmin; zz <= zmax; zz++)
                        {
                            if (map.Get(xx,zz,CellIndex.Terrain) < 1)
                            {
                                continue;
                            }

                            int bits = map.Get(xx, zz, CellIndex.Magic);
                            bits |= (1 << (int)(mtype.IdKey - 1));
                            map.Set(xx, zz, CellIndex.Magic, bits);
                        }
                    }
                }
            }

            return new NewCrawlerMap() { Map = map, EnterX = enterX, EnterZ = enterZ };
        }


        protected long GetRandomEncounter(IRandom rand)        
        {

            MapEncounterType encounter = RandomUtils.GetRandomElement(_gameData.Get<MapEncounterSettings>(_gs.ch).GetData(), rand);

            if (encounter != null)
            {
                return encounter.IdKey;
            }

            return 0;

        }
        const float extraLengthChance = 0.25f;
        protected int GetRoomDeltaSize(IRandom rand, int roomEdgeDist)
        {
            int retval = 1;

            for (int i = 0; i < 3; i++)
            {
                if (retval >= roomEdgeDist-2)
                {
                    return retval;
                }
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

        protected void ConnectOpenCells(CrawlerMap map, CrawlerMapGenData genData, IRandom rand)
        {

            bool[,] openCell = new bool[map.Width, map.Height];

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    openCell[x, z] = map.Get(x, z, CellIndex.Terrain) >= 0;
                }
            }

            while (true)
            {
                bool hadDisconnectedCell = false;

                bool[,] connectedCells = new bool[map.Width, map.Height];

                Queue<PointXZ> cellsToCheck = new Queue<PointXZ>();

                cellsToCheck.Enqueue(new PointXZ(map.Width / 2, map.Height / 2));

                while (cellsToCheck.Count > 0)
                {
                    PointXZ currentCell = cellsToCheck.Dequeue();

                    int x = currentCell.X;
                    int z = currentCell.Z;

                    connectedCells[x, z] = true;

                    // If x on right or map loops, see if there's a disconnected cell to east.
                    if (x < map.Width - 1 || map.Looping)
                    {
                        int nx = (x + 1) % map.Width;
                        if (!connectedCells[nx, z] && !WallTypes.IsBlockingType(map.EastWall(x, z)))
                        {
                            connectedCells[nx, z] = true;
                            cellsToCheck.Enqueue(new PointXZ(nx, z));
                        }
                    }
                    if (x > 0 || map.Looping)
                    {
                        int nx = (x - 1 + map.Width) % map.Width;
                        if (!connectedCells[nx, z] && !WallTypes.IsBlockingType(map.EastWall(nx, z)))
                        {
                            connectedCells[nx, z] = true;
                            cellsToCheck.Enqueue(new PointXZ(nx, z));
                        }
                    }

                    if (z < map.Height - 1 || map.Looping)
                    {
                        int nz = (z + 1) % map.Height;
                        if (!connectedCells[x, nz] && !WallTypes.IsBlockingType(map.NorthWall(x, z)))
                        {
                            connectedCells[x, nz] = true;
                            cellsToCheck.Enqueue(new PointXZ(x, nz));
                        }
                    }
                    if (z > 0 || map.Looping)
                    {
                        int nz = (z - 1 + map.Height) % map.Height;
                        if (!connectedCells[x, nz] && !WallTypes.IsBlockingType(map.NorthWall(x, nz)))
                        {
                            connectedCells[x, nz] = true;
                            cellsToCheck.Enqueue(new PointXZ(x, nz));
                        }
                    }
                }

                for (int x = 0; x < map.Width; x++)
                {
                    for (int z = 0; z < map.Height; z++)
                    {
                        if (openCell[x, z] && !connectedCells[x, z])
                        {
                            hadDisconnectedCell = true;

                            if (rand.NextDouble() > 0.1f)
                            {
                                continue;
                            }

                            long bits = map.Get(x, z, CellIndex.Walls);

                            if (x < map.Width - 1 || genData.Looping)
                            {
                                bits &= ~(WallTypes.Wall << MapWallBits.EWallStart);
                            }
                            if (z < map.Height - 1 || genData.Looping)
                            {
                                bits &= ~(WallTypes.Wall << MapWallBits.NWallStart);
                            }

                            map.Set(x, z, CellIndex.Walls, (byte)bits);

                            if (x > 0 || genData.Looping)
                            {
                                int nx = (x + map.Width - 1) % map.Width;

                                long ebits = map.Get(nx, z, CellIndex.Walls);

                                ebits &= ~(WallTypes.Wall << MapWallBits.EWallStart);
                                map.Set(nx, z, CellIndex.Walls, (byte)ebits);
                            }
                            if (z > 0 || genData.Looping)
                            {
                                int nz = (z + map.Height - 1) % map.Height;
                                long nbits = map.Get(x, nz, CellIndex.Walls);
                                nbits &= ~(WallTypes.Wall << MapWallBits.NWallStart);
                                map.Set(x, nz, CellIndex.Walls, (byte)nbits);
                            }

                        }
                    }
                }

                if (!hadDisconnectedCell)
                {
                    break;
                }
            }
        }
    }
}
