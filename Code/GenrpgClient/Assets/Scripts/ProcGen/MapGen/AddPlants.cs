
using System;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;
using Genrpg.Shared.ProcGen.Settings.Plants;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using Assets.Scripts.ProcGen.Loading.Utils;

public class BaseDetailPrototype
{
    public ZonePlantType zonePlant = null;
    public PlantType plantType = null;
    public long noiseSeed = 0;
    public int Index = 0;
    public int XGrid = -1;
    public int YGrid = -1;
    public List<long> zoneIds = new List<long>();
}

public class AddPlants : BaseZoneGenerator
{
    public const float GrassBaseScale = 1.0f;
    public const float GrassDensityScale = 1.0f;
    public const float GrassRandomChance = 0.005f;

    public const float GrassFreqScale = 1.0f;

    private IZonePlantValidator _zonePlantValidator;

    public override async UniTask Generate(CancellationToken token)
    {
        await base.Generate(token);
        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            GenerateOne(zone, _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId), zone.XMin, zone.ZMin, zone.XMax, zone.ZMax);
        }

        AddPlantsToMapData(_gs);
    }



   

    public void GenerateOne(Zone zone, ZoneType zoneType, int startx, int starty, int endx, int endy)
    {
        if (startx >= endx || starty >= endy)
        {
            return;
        }

        List<ZonePlantType> plist = zone.PlantTypes;
        if (plist == null)
        {
            return;
        }

        List<FullDetailPrototype> fullList = new List<FullDetailPrototype>();

        _zonePlantValidator.UpdateValidPlantTypeList(zone, -1, -1, fullList,true, _token);

        if (fullList == null)
        {
            return;
        }

        int dx = endx - startx + 1;
        int dy = endy - starty + 1;
        int perlinSize = Math.Max(MapConstants.DefaultNoiseSize, Math.Max(dx, dy));
        float perlinScale = perlinSize * 1.0f / MapConstants.DefaultHeightmapSize;
        while (fullList.Count > MapConstants.MaxGrass)
        {
            fullList.RemoveAt(fullList.Count - 1);
        }

        for (int index = 0; index < fullList.Count; index++)
        {
            FullDetailPrototype full = fullList[index];

            if (full.noiseSeed == 0 || full.plantType == null || full.zonePlant == null)
            {
                continue;
            }
            long pseed = full.noiseSeed;

            int plantChanceTimes = 2;

            List<float[,]> plantChances = new List<float[,]>();

            float density = 0.0f;

            float midSteepVal = 70f;

            GenZone genZone = _md.GetGenZone(zone.IdKey);

            MyRandom rand = new MyRandom(full.noiseSeed);
            for (int i = 0; i < plantChanceTimes; i++)
            {
                float pers = MathUtils.FloatRange(0.1f, 0.3f, rand) * 1.2f;
                float amp = MathUtils.FloatRange(1.0f, 2.0f, rand) * 0.9f;
                float freq = perlinSize * MathUtils.FloatRange(0.04f, 0.25f, rand);

                int octaves = 2;


                freq *= genZone.GrassFreq * zoneType.GrassFreq * GrassFreqScale;

                freq *= perlinScale;


                if (freq < 8)
                {
                    freq = 8;
                }

                density = genZone.GrassDensity * zoneType.GrassDensity * GrassDensityScale;

                amp *= density;
                
              

                plantChances.Add(_noiseService.Generate(pers, freq, amp, octaves, pseed, perlinSize, perlinSize));
            }

            float steepFreq = perlinSize * MathUtils.FloatRange(0.05f, 0.15f, rand);
            float steepAmp = MathUtils.FloatRange(4, 20, rand);
            float steepPers = MathUtils.FloatRange(0.1f, 0.3f, rand);
            int steepOctaves = 2;

            /// Steepness allowed at each coord for this grass to grow or not.
            float[,] steepVals = _noiseService.Generate(steepPers, steepFreq, steepAmp, steepOctaves, pseed + 1, perlinSize, perlinSize);

            int numChecked = 0;
            int badZoneId = 0;
            int nearRoad = 0;
            int nearLocation = 0;
            int didSet = 0;

            bool useUniformDensity = false;
            for (int x = startx; x <= endx; x++)
            {
                for (int y = starty; y <= endy; y++)
                {
                    float currDensityMult = MathUtils.FloatRange(0, 2, rand);
                    numChecked++;

                    if (_md.mapZoneIds[x,y] != zone.IdKey) // zoneobject
                    {
                        badZoneId++;
                        continue;
                    }
                    if (_md.mapObjects[x,y] != 0)
                    {
                        continue;
                    }
                    if (FlagUtils.IsSet(_md.flags[x, y], MapGenFlags.BelowWater))
                    {
                        continue;
                    }
                    if (_md.alphas[x,y, MapConstants.RoadTerrainIndex] > 0)
                    {
                        bool isNearRoad = false;
                        int roadRad = 0;
                        for (int xx = x - roadRad; xx <= x + roadRad; xx++)
                        {
                            if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
                            {
                                continue;
                            }
                            for (int yy = y - roadRad; yy <= y + roadRad; yy++)
                            {
                                if (yy < 0 || yy >= _mapProvider.GetMap().GetHhgt())
                                {
                                    continue;
                                }
                                if (_md.alphas[xx,yy, MapConstants.RoadTerrainIndex] > 0)
                                {
                                    isNearRoad = true;
                                    break;
                                }
                            }
                        }
                        if (isNearRoad)
                        {
                            nearRoad++;
                            continue;
                        }
                    }
                    float hgt = _terrainManager.SampleHeight(x, y);
                    if (hgt < MapConstants.MinLandHeight*7/10)
                    {
                        continue;
                    }


                    float steep = _terrainManager.GetSteepness(x,y);

                    if (steep > (midSteepVal+steepVals[x-startx,y-starty]))
                    {
                        continue;
                    }

                    float chance = 0;
                    if (!full.plantType.HasFlag(PlantFlags.SmallPatches) && !useUniformDensity)
                    {

                        for (int i = 0; i < plantChanceTimes; i++)
                        {
                            float origChance = plantChances[i][x - startx, y - starty];
                            if (origChance < 0)
                            {
                                origChance = -origChance / 2;
                            }
                            chance += origChance;
                        }

                        chance /= plantChanceTimes;

                        if (chance > 1)
                        {
                            chance = 1;
                        }
                    }
                    else
                    {
                        if (_rand.NextDouble() > currDensityMult*density / 20.0f)
                        {
                            continue;
                        }
                        else
                        {
                            chance = MathUtils.FloatRange(0, 1, rand);
                        }
                    }

                    if (_zoneGenService.FindMapLocation(x, y, 1) != null)
                    {
                        if (FlagUtils.IsSet(_md.flags[x, y], MapGenFlags.IsLocationPatch))
                        {
                            nearLocation++;
                            continue;
                        }
                    }


                    short val = (short)(chance * MapConstants.MaxGrassValue);

                    if (val < 1 && rand.NextDouble() < GrassRandomChance)
                    {
                        val = (short)MathUtils.IntRange(1, 3, rand);
                    }

                    if (val > 0)
                    {
                        didSet++;

                        val = (short)Math.Max(steep / 10, val);
                        if (val > MapConstants.MaxGrassValue)
                        {
                            val = MapConstants.MaxGrassValue;
                        }

                        if (val < 3)
                        {
                            val++;
                        }

                        int ny = y - (y / (MapConstants.TerrainPatchSize - 1))*0;
                        int nx = x - (x / (MapConstants.TerrainPatchSize - 1))*0;
                        if (full.plantType.HasFlag(PlantFlags.UsePrefab))
                        {                            
                            val = (short)(val * MapConstants.PrefabPlantDensityScale);
                        }
                        _md.grassAmounts[nx, ny, index] = (byte)val;
                    }
                }

            }
        }
    }

    public void AddPlantsToMapData(IUnityGameState gs)
    {

        if (base._md.grassAmounts == null || base._md.mapObjects == null)
        {
            return;
        }
        for (int x = 0; x < _mapProvider.GetMap().GetHwid(); x++)
        {
            for (int y = 0; y < _mapProvider.GetMap().GetHhgt(); y++)
            {
                if (base._md.mapObjects[x, y] == 0)
                {
                    int val = 0;
                    int[] vals = new int[MapConstants.MaxGrass];
                    for (int i = 0; i < MapConstants.MaxGrass; i++)
                    {
                        int currVal = Math.Min(MapConstants.MaxGrassValue, (int)base._md.grassAmounts[x, y, i]);
                        vals[i] = currVal;
                        for (int j = 0; j < i; j++)
                        {
                            currVal *= (MapConstants.MaxGrassValue + 1);
                        }
                        val += currVal;
                    }
                    if (val != 0)
                    {
                        base._md.mapObjects[x, y] = (ushort)(MapConstants.GrassMinCellValue + val);
                    }
                }
            }
        }
    }

}

