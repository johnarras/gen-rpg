
using System.Collections.Generic;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.ProcGen.Entities;
using System.Linq;
using System.Threading;
using Genrpg.Shared.Utils;
using UnityEngine;

public class AddZoneCenters : BaseZoneGenerator
{
    protected ISamplingService _sampleService;

    public const int WallPatchId = 1;
	public override async Awaitable Generate (CancellationToken token)
    {
        await base.Generate(token);
        SamplingData sdata = new SamplingData();
        _md.zoneCenters = new List<MyPoint>();
        float edgeSize = MapConstants.TerrainPatchSize*3/4;

        float blockSize = MapConstants.TerrainPatchSize;

        blockSize = _mapProvider.GetMap().ZoneSize * MapConstants.TerrainPatchSize;

        int totalSize = _mapProvider.GetMap().GetHwid();
        float searchSize = _mapProvider.GetMap().GetHwid() - edgeSize;

        if (searchSize < totalSize/2)
        {
            searchSize = totalSize / 2;
        }

        sdata.Count = (int)((0.45f * totalSize*totalSize) / (blockSize * blockSize));
        if (sdata.Count < 1)
        {
            sdata.Count = 1;

        }

        if (_mapProvider.GetMap().IsSingleZone())
        {
            sdata.Count = 1;
        }

        _logService.Info("Map TotalSize: " + totalSize + " SearchSize: " + searchSize + " BlockSize: " + blockSize);

        sdata.MaxAttemptsPerItem = 1000;
        sdata.MinSeparation = blockSize * 12 / 10;



        sdata.XMin = -blockSize*2;
        sdata.XMax = _mapProvider.GetMap().GetHwid() + blockSize*2;
        sdata.YMin = -blockSize*2;
        sdata.YMax = _mapProvider.GetMap().GetHhgt() + blockSize*2;
        sdata.Seed = _mapProvider.GetMap().Seed % 1000000000 + 3824821;

        sdata.NoiseAmp = MathUtils.FloatRange(0.3f, 0.8f, _rand);
        sdata.NoiseFreq = MathUtils.FloatRange(3.0f, 10.0f, _rand);

        List<MyPoint2> centers = _sampleService.PlanePoissonSample(sdata);
        
        centers = centers.Where(p => 
        p.X >= edgeSize
        && p.Y >= edgeSize
        && p.X <= _mapProvider.GetMap().GetHwid() - edgeSize
        && p.Y <= _mapProvider.GetMap().GetHhgt() - edgeSize).ToList();

        _logService.Info("Centers Wanted: " + sdata.Count + " Found: " + centers.Count);

        if (centers.Count < 1)
        {
            MyPoint2 center = new MyPoint2(_mapProvider.GetMap().GetHwid() / 2, _mapProvider.GetMap().GetHhgt() / 2);
            centers.Add(center);
        }
        if (centers.Count > 0)
        {
            for (int c =0; c < centers.Count; c++)
            {
                MyPoint2 center = centers[c];
                _md.zoneCenters.Add(new MyPoint((int)center.X, (int)center.Y));
            }
        }
  	}
}
	
