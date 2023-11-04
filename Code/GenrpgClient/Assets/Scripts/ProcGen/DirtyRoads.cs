using System;
using System.Threading.Tasks;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using System.Threading;
using Genrpg.Shared.ProcGen.Entities;

public class DirtyRoads : BaseZoneGenerator
{
    public override async Task Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        foreach (Zone zone in gs.map.Zones)
        {
            GenerateOne(gs, zone, gs.data.GetGameData<ZoneTypeSettings>(gs.ch).GetZoneType(zone.ZoneTypeId), zone.XMin, zone.XMax, zone.ZMin, zone.ZMax);
        }
        await Task.CompletedTask;
    }

    public void GenerateOne (UnityGameState gs, Zone zone, ZoneType zoneType, int minx, int maxx, int miny, int maxy)
    {
        if (zone == null)
        {
            return;
        }

        int sizex = maxx - minx + 1;
        int sizey = maxy - miny + 1;

        if (sizex < 10 || sizey< 10)
        {
            return;
        }

        int size = Math.Max(sizex, sizey);

        MyRandom rand = new MyRandom(zone.Seed+724334);

        float globalScale = 1.25f;

        float amp = MathUtils.FloatRange(0.6f, 1.3f, rand)*globalScale;
        float freq = MathUtils.FloatRange(0.2f, 0.3f, rand) * size*globalScale;
        float pers = MathUtils.FloatRange(0.4f, 0.7f, rand)*globalScale;
		int octaves = 2;

        float[,] dirtHeights = _noiseService.Generate(gs, pers, freq, amp, octaves, rand.Next(), sizex, sizey);

        float minRoadPercent = 0.20f;

		float maxOtherPercent = 0.80f;

        amp = MathUtils.FloatRange(0.6f, 1.3f, rand)*globalScale;
        freq = MathUtils.FloatRange(0.15f, 0.25f, rand) * size * globalScale;
        pers = MathUtils.FloatRange(0.4f, 0.7f, rand) * globalScale;
        octaves = 2;
        float[,] baseHeights = _noiseService.Generate(gs, pers, freq, amp, octaves, rand.Next(), sizex, sizey);


        float startMaxPct = 0.85f;

        float pctamp = MathUtils.FloatRange(0.2f,0.3f, rand)* globalScale;
        float pctfreq = MathUtils.FloatRange(0.1f, 0.2f, rand) * size * globalScale;
        float pctpers = MathUtils.FloatRange(0.0f, 0.4f, rand) * globalScale;
        int pctoctaves = 2;
        float[,] maxPcts = _noiseService.Generate(gs, pctpers, pctfreq, pctamp, pctoctaves, rand.Next(), sizex, sizey);


        float generalPerturb = 0.1f;

        for (int x = minx; x <= maxx; x++)
		{
            if (x < 0 || x >= gs.map.GetHwid())
            {
                continue;
            }

			for (int z = miny; z <= maxy; z++)
			{
                if (z < 0 || z >= gs.map.GetHhgt())
                {
                    continue;
                }

				if (gs.md.alphas[x,z,MapConstants.RoadTerrainIndex] < minRoadPercent)
                {
                    continue;
                }

                if (gs.md.mapZoneIds[x,z] != zone.IdKey)
                {
                    continue;
                }

				// Get height > 0
				//float dirtPct = Math.Abs (dirtHeights[x,z]);
				//float basePct = Math.Abs (baseHeights[x,z]);
				float dirtPct = MathUtils.Clamp(0,dirtHeights[x-minx,z-miny]+MathUtils.FloatRange(-generalPerturb,generalPerturb,rand),maxOtherPercent);
				float basePct = MathUtils.Clamp(0,baseHeights[x-minx,z-miny]+MathUtils.FloatRange(-generalPerturb,generalPerturb,rand),maxOtherPercent);

                


				float totalPct = dirtPct+basePct;


                float maxPct = maxPcts[x - minx, z - miny] + startMaxPct;

				if (totalPct > maxPct)
				{
					dirtPct /= (totalPct/maxPct);
					basePct /= (totalPct/maxPct);
					totalPct = maxPct;
				}

				gs.md.ClearAlphasAt(gs,x,z);
				gs.md.alphas[x,z,MapConstants.RoadTerrainIndex] = 1-totalPct;
				gs.md.alphas[x,z,MapConstants.DirtTerrainIndex] = dirtPct;
				gs.md.alphas[x,z,MapConstants.BaseTerrainIndex] = basePct;

			}
		}
	}
}