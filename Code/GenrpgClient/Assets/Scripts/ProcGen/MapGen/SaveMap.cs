using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Login.Messages.UploadMap;
using Assets.Scripts.MapTerrain;
using System;
using Genrpg.Shared.Utils;

public class SaveMap : BaseZoneGenerator
{
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);

        gs.map.OverrideZonePercent = 0; // MathUtils.IntRange(20, 80, gs.rand);

        for (int gx = 0; gx < gs.map.BlockCount; gx++)
        {
            for (int gy = 0; gy < gs.map.BlockCount; gy++)
            {
                SaveOneTerrainPatch(gs, gx, gy);
            }
        }
    }


    public void SaveOneTerrainPatch(UnityGameState gs, int gx, int gy)
    {

        TerrainPatchData patch = _terrainManager.GetTerrainPatch(gs, gx, gy);

        if (patch == null)
        {
            _logService.Error("NO patch at " + gx + " " + gy);
            return;
        }

        if (patch.FullZoneIdList == null || patch.FullZoneIdList.Count < 1)
        {
            _logService.Error("No zone list at " + gx + " " + gy);
            return;
        }



        int maxFileSize = MapConstants.TerrainPatchSize * MapConstants.TerrainPatchSize * MapConstants.TerrainBytesPerUnit;

        byte[] bytes = new byte[maxFileSize];
        int startX = gy * (MapConstants.TerrainPatchSize - 1);
        int startY = gx * (MapConstants.TerrainPatchSize - 1);


        ushort shortHeight = 0;
        int worldObjectValue = 0;
        int index = 0;

        // 1. Heights 2
        // 2. Objects 4
        // 3. Alphas 3
        // 4. Zone 1 
        // 5. SubZone 1
        // 6. OverrideZoneScale 1

        // 2 + 4 + 3 + 1 + 1 + 1 = 12;

        // 1 Heights: 2 bytes
        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
            {
                shortHeight = (ushort)(MapConstants.HeightSaveMult * gs.md.heights[x + startX, y + startY]);

                bytes[index++] = (byte)(shortHeight);
                bytes[index++] = (byte)(shortHeight >> 8);
            }
        }

        int objStartX = gx * (MapConstants.TerrainPatchSize - 1);
        int objStartY = gy * (MapConstants.TerrainPatchSize - 1);

        // 2 Objects: 4 bytes
        for (int x = 0; x < MapConstants.TerrainPatchSize - 1; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize - 1; y++)
            {
                int xx = x + objStartX;
                int yy = y + objStartY;
                //worldObjectValue = gs.md.mapObjects[x + objStartX, y + objStartY];
                worldObjectValue = gs.md.mapObjects[y + objStartY, x + objStartX];

                if (x == MapConstants.TerrainPatchSize - 1 || y == MapConstants.TerrainPatchSize - 1)
                {
                    if (worldObjectValue < MapConstants.GrassMinCellValue ||
                        worldObjectValue > MapConstants.GrassMaxCellValue)
                    {
                        worldObjectValue = 0;
                    }
                }

                bytes[index++] = (byte)(worldObjectValue);
                bytes[index++] = (byte)(worldObjectValue >> 8);
                bytes[index++] = (byte)(worldObjectValue >> 16);
                bytes[index++] = (byte)(worldObjectValue >> 24);

            }
        }
        // 3 Alphas: 3 bytes (*divsq)
        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
            {

                for (int i = 0; i < MapConstants.MaxTerrainIndex - 1; i++)
                {
                    bytes[index++] = (byte)(gs.md.alphas[x + startX, y + startY, i] * MapConstants.AlphaSaveMult);
                }
            }
        }


        List<long> zoneIds = new List<long>();
        // 4 Zones: 1 byte (*divsq)
        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
            {
                byte zid = (byte)gs.md.mapZoneIds[x + startX, y + startY];
                if (zid <= MapConstants.MountainZoneId)
                {
                    _logService.Error("Found bad zoneId at " + (x + startX) + " " + (y + startY));
                }
                bytes[index++] = zid;
                if (!zoneIds.Contains(zid))
                {
                    zoneIds.Add(zid);
                }
            }
        }
        // 5 subZoneIds 1 byte
        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
            {
                bytes[index++] = (byte)(gs.md.subZoneIds[x + startX, y + startY]);
            }
        }


        // 6 OverrideZoneScale 1 byte 0 to MapConstants.OverrideZoneScaleMax
        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
            {
                float val = MathUtils.Clamp(0, Math.Abs(gs.md.overrideZoneScales[x + startX, y + startY]), 1);

                bytes[index++] = (byte)(val*MapConstants.OverrideZoneScaleMax);
            }
        }

        byte[] newBytes = new byte[index];
        for (int i = 0; i < index; i++)
        {
            newBytes[i] = bytes[i];
        }

        ClientRepositoryCollection<TerrainPatchData> repo = new ClientRepositoryCollection<TerrainPatchData>(_logService);

        string zoneText = "";
        foreach (long zid in zoneIds)
        {
            zoneText += zid + " ";
        }
        repo.SaveBytes(patch.GetFilePath(gs, true), newBytes);

    }
}
	
