using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.MapServer.Constants;
using Assets.Scripts.MapTerrain;

public class SetupTerrainPatches : BaseZoneGenerator
{
    public override async Task Generate(UnityGameState gs, CancellationToken token)
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
                        }
                        int baseZoneId = gs.md.subZoneIds[y, x];

                        if (baseZoneId >= SharedMapConstants.MinBaseZoneId && !patch.FullZoneIdList.Contains(baseZoneId))
                        {
                            patch.FullZoneIdList.Add(baseZoneId);
                        }

                        if (y - sy < MapConstants.TerrainPatchSize && x - sx < MapConstants.TerrainPatchSize)
                        {
                            patch.mainZoneIds[y - sy, x - sx] = gs.md.mapZoneIds[y, x];
                            patch.subZoneIds[y - sy, x - sx] = gs.md.subZoneIds[y, x];
                        }
                    }
                };
            }
        }
    }
}
