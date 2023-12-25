using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.MapServer.Constants;
using Assets.Scripts.MapTerrain;
using System.Linq;

public class SetupTerrainPatches : BaseZoneGenerator
{
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);

        for (int px = 0; px < MapConstants.MaxTerrainGridSize; px++)
        {
            for (int py = 0; py < MapConstants.MaxTerrainGridSize; py++)
            {
                if (gs.md.terrainPatches[px, py] == null)
                {
                    gs.md.SetTerrainPatchAtGridLocation(gs, px, py, null, null);
                }

                UnityEngine.TerrainData tdata = gs.md.GetTerrainData(gs, px, py);
                if (tdata == null)
                {
                    continue;
                }

                TerrainPatchData patch = gs.md.terrainPatches[px, py];
                int sx = px * (MapConstants.TerrainPatchSize - 1);
                int sy = py * (MapConstants.TerrainPatchSize - 1);

                Dictionary<int, int> baseZoneIdCounts = new Dictionary<int, int>();

                for (int y = sy; y <= sy + MapConstants.TerrainPatchSize && y < gs.map.GetHhgt(); y++)
                {
                    for (int x = sx; x <= sx + MapConstants.TerrainPatchSize && x < gs.map.GetHwid(); x++)
                    {
                        int zoneId = gs.md.mapZoneIds[y, x];
                        if (zoneId < SharedMapConstants.MapZoneStartId)
                        {
                            gs.logger.Message("Missing zoneId at " + x + " " + y);
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
                        int baseZoneId = gs.md.subZoneIds[y, x];

                        if (baseZoneId >= SharedMapConstants.MinBaseZoneId && !patch.FullZoneIdList.Contains(baseZoneId))
                        {
                            patch.FullZoneIdList.Add(baseZoneId);
                        }

                        if (y - sy < MapConstants.TerrainPatchSize && x - sx < MapConstants.TerrainPatchSize)
                        {
                            patch.mainZoneIds[y - sy, x - sx] = (byte)gs.md.mapZoneIds[y, x];
                            patch.subZoneIds[y - sy, x - sx] = (byte)gs.md.subZoneIds[y, x];
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
