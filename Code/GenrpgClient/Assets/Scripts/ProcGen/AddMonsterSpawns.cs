
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using GEntity = UnityEngine.GameObject;


using Genrpg.Shared.Core.Entities;



using Cysharp.Threading.Tasks;

using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.Pathfinding.Constants;
using System.Threading;
using Genrpg.Shared.ProcGen.Entities;

public class AddMonsterSpawns : BaseZoneGenerator
{

    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        foreach (Zone zone in gs.map.Zones)
        {
            GenerateOne(gs, zone, gs.data.GetGameData<ZoneTypeSettings>(gs.ch).GetZoneType(zone.ZoneTypeId), zone.XMin, zone.ZMin, zone.XMax, zone.ZMax);
        }
    }
    public void GenerateOne(UnityGameState gs, Zone zone, ZoneType zoneType, int startx, int starty, int endx, int endy)
    {
        if ( zone == null || zoneType == null || startx >= endx || starty >= endy ||
            gs.map == null)
        {
            return;
        }

        startx = MathUtils.Clamp(MapConstants.MapEdgeSize, startx, gs.map.GetHwid() - MapConstants.MapEdgeSize);
        starty = MathUtils.Clamp(MapConstants.MapEdgeSize, starty, gs.map.GetHhgt() - MapConstants.MapEdgeSize);


        endx = MathUtils.Clamp(MapConstants.MapEdgeSize, endx, gs.map.GetHwid() - MapConstants.MapEdgeSize);
        endy = MathUtils.Clamp(MapConstants.MapEdgeSize, endy, gs.map.GetHhgt() - MapConstants.MapEdgeSize);

        MyRandom rand = new MyRandom(zone.Seed + 1);

        int minZoneDist = 4;
        int zoneCheckSkip = 3;

        int offsetSize = MapConstants.MonsterSpawnSkipSize / 4;

        for (int x = startx; x <= endx; x += MapConstants.MonsterSpawnSkipSize)
        {
            for (int y = starty; y <= endy; y += MapConstants.MonsterSpawnSkipSize)
            {
                int cx = x + MathUtils.IntRange(-offsetSize, offsetSize, rand);
                int cy = y + MathUtils.IntRange(-offsetSize, offsetSize, rand);

                if (cx < 0 || cy < 0 || cx >= gs.map.GetHwid() || cy >= gs.map.GetHhgt())
                {
                    continue;
                }

                if (cx < 0 || cy < 0 || cx >= gs.map.GetHwid() || cy >= gs.map.GetHhgt())
                {
                    continue;
                }
                if (FlagUtils.IsSet(gs.md.flags[cx, cy], MapGenFlags.BelowWater))
                {
                    continue;
                }

                if (!isValidCell(gs.md.mapObjects[cx, cy]))
                {
                    continue;
                }

                if (gs.md.mountainHeights[cx, cy] >= 1.0f)
                {
                    continue;
                }

                if (gs.md.mountainDistPercent[cx,cy] < 0.1f)
                {
                    continue;
                }

                if (gs.md.GetSteepness(gs, cx, cy) > PathfindingConstants.MaxSteepness)
                {
                    continue;
                }

                if (gs.md.heights[cx, cy] <= (MapConstants.MinLandHeight*7/10) / MapConstants.MapHeight)
                {
                    continue;
                }


                if (gs.md.bridgeDistances[cx,cy] < 20)
                {
                    continue;
                }

                if (_zoneGenService.FindMapLocation(gs, cx, cy, 5) != null)
                {
                    continue;
                }


                if (gs.md.roadDistances[cx,cy] < 4)
                {
                    continue;
                }
                bool nearAnotherZone = false;
                for (int xx = cx - minZoneDist; xx <= cx + minZoneDist; xx += zoneCheckSkip)
                {
                    if (xx < 0 || xx >= gs.map.GetHwid())
                    {
                        continue;
                    }
                    for (int yy = cy-minZoneDist; yy <= cy+minZoneDist; yy+=zoneCheckSkip)
                    {
                        if (yy < 0 || yy >= gs.map.GetHhgt())
                        {
                            continue;
                        }
                        if (gs.md.mapZoneIds[xx,yy] != zone.IdKey)
                        {
                            nearAnotherZone = true;
                            break;
                        }
                    }

                    if (nearAnotherZone)
                    {
                        break;
                    }
                }

                if (nearAnotherZone)
                {
                    continue;
                }

                long zoneId = zone.IdKey;
                if (gs.md.subZoneIds[cx,cy] > 0)
                {
                    zoneId = gs.md.subZoneIds[cx, cy];
                }

                gs.spawns.AddSpawn(EntityTypes.ZoneUnit, zoneId, cy, cx, zone.IdKey, 
                    (int)(gs.md.overrideZoneScales[cy, cx] * MapConstants.OverrideZoneScaleMax));
            }
        }
    }

    protected bool isValidCell (int value)
    {
        return value == 0 ||
            (value >= MapConstants.GrassMinCellValue && value < MapConstants.GrassMaxCellValue);
    }

}

