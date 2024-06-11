using System.Collections.Generic;

using System.Threading;
using Genrpg.Shared.MapServer.Constants;
using Assets.Scripts.MapTerrain;
using System.Linq;
using UnityEngine;

public class SetupTerrainPatches : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);

        for (int px = 0; px < MapConstants.MaxTerrainGridSize; px++)
        {
            for (int py = 0; py < MapConstants.MaxTerrainGridSize; py++)
            {
                if (_terrainManager.GetTerrainPatch(px, py, false) == null)
                {
                    _terrainManager.SetTerrainPatchAtGridLocation(px, py, null, null);
                }

                UnityEngine.TerrainData tdata = _terrainManager.GetTerrainData(px, py);
                if (tdata == null)
                {
                    continue;
                }

                TerrainPatchData patch = _terrainManager.GetTerrainPatch(px, py, false);
                int sx = px * (MapConstants.TerrainPatchSize - 1);
                int sy = py * (MapConstants.TerrainPatchSize - 1);

                Dictionary<int, int> baseZoneIdCounts = new Dictionary<int, int>();

                for (int y = sy; y <= sy + MapConstants.TerrainPatchSize && y < _mapProvider.GetMap().GetHhgt(); y++)
                {
                    for (int x = sx; x <= sx + MapConstants.TerrainPatchSize && x < _mapProvider.GetMap().GetHwid(); x++)
                    {
                        int zoneId = _md.mapZoneIds[y, x];
                        if (zoneId < SharedMapConstants.MapZoneStartId)
                        {
                            _logService.Message("Missing zoneId at " + x + " " + y);
                        }
                        else if (!patch.FullZoneIdList.Contains(zoneId))
                        {
                            patch.FullZoneIdList.Add(zoneId);
                            if (!baseZoneIdCounts.ContainsKey(zoneId))
                            {
                                baseZoneIdCounts[zoneId] = 0;
                            }
                            baseZoneIdCounts[zoneId]++;
                        }
                        int baseZoneId = _md.subZoneIds[y, x];

                        if (baseZoneId >= SharedMapConstants.MinBaseZoneId && !patch.FullZoneIdList.Contains(baseZoneId))
                        {
                            patch.FullZoneIdList.Add(baseZoneId);
                        }

                        if (y - sy < MapConstants.TerrainPatchSize && x - sx < MapConstants.TerrainPatchSize)
                        {
                            patch.mainZoneIds[y - sy, x - sx] = (byte)_md.mapZoneIds[y, x];
                            patch.subZoneIds[y - sy, x - sx] = (byte)_md.subZoneIds[y, x];
                        }
                    }
                };

                if (baseZoneIdCounts.Values.Count > 0)
                {
                    int maxZoneIdCount = baseZoneIdCounts.Values.Max();

                    int biggestZoneId = -1;

                    foreach (int zid in baseZoneIdCounts.Keys)
                    {
                        if (baseZoneIdCounts[zid] == maxZoneIdCount)
                        {
                            biggestZoneId = zid;
                            break;
                        }
                    }

                    // Place this first so it's the only thing we look at for the purposes of grass.
                    patch.FullZoneIdList.Remove(biggestZoneId);
                    patch.FullZoneIdList.Insert(0, biggestZoneId);
                }
            }
        }
    }
}
