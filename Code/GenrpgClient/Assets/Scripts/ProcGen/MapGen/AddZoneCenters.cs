
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.ProcGen.Entities;

using System.Linq;
using System.Threading;
using Genrpg.Shared.ProcGen.Settings.Locations;
using Genrpg.Shared.Utils;

public class AddZoneCenters : BaseZoneGenerator
{
    protected ISamplingService _sampleService;

    public const int WallPatchId = 1;
	public override async UniTask Generate (UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        SamplingData sdata = new SamplingData();
        gs.md.zoneCenters = new List<MyPoint>();
        float edgeSize = MapConstants.TerrainPatchSize*3/4;

        float blockSize = MapConstants.TerrainPatchSize;

        blockSize = gs.map.ZoneSize * MapConstants.TerrainPatchSize;

        int totalSize = gs.map.GetHwid();
        float searchSize = gs.map.GetHwid() - edgeSize;

        if (searchSize < totalSize/2)
        {
            searchSize = totalSize / 2;
        }

        sdata.Count = (int)((0.45f * totalSize*totalSize) / (blockSize * blockSize));
        if (sdata.Count < 1)
        {
            sdata.Count = 1;

        }

        if (gs.map.IsSingleZone(gs))
        {
            sdata.Count = 1;
        }

        gs.logger.Info("Map TotalSize: " + totalSize + " SearchSize: " + searchSize + " BlockSize: " + blockSize);

        sdata.MaxAttemptsPerItem = 1000;
        sdata.MinSeparation = blockSize * 12 / 10;



        sdata.XMin = -blockSize*2;
        sdata.XMax = gs.map.GetHwid() + blockSize*2;
        sdata.YMin = -blockSize*2;
        sdata.YMax = gs.map.GetHhgt() + blockSize*2;
        sdata.Seed = gs.map.Seed % 1000000000 + 3824821;

        List<MyPoint2> centers = _sampleService.PlanePoissonSample(gs, sdata);
        
        centers = centers.Where(p => 
        p.X >= edgeSize
        && p.Y >= edgeSize
        && p.X <= gs.map.GetHwid() - edgeSize
        && p.Y <= gs.map.GetHhgt() - edgeSize).ToList();

        gs.logger.Info("Centers Wanted: " + sdata.Count + " Found: " + centers.Count);

        if (centers.Count < 1)
        {
            MyPoint2 center = new MyPoint2(gs.map.GetHwid() / 2, gs.map.GetHhgt() / 2);
            centers.Add(center);
        }
        if (centers.Count > 0)
        {
            for (int c =0; c < centers.Count; c++)
            {
                MyPoint2 center = centers[c];
                gs.md.zoneCenters.Add(new MyPoint((int)center.X, (int)center.Y));
            }
        }
  	}
}
	
