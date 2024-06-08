using System;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using System.Threading;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;

public class DirtyRoads : BaseZoneGenerator
{
    public override async UniTask Generate(CancellationToken token)
    {
        await base.Generate(token);
        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            GenerateOne(zone, _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId), zone.XMin, zone.XMax, zone.ZMin, zone.ZMax);
        }
        await UniTask.CompletedTask;
    }

    public void GenerateOne (Zone zone, ZoneType zoneType, int minx, int maxx, int miny, int maxy)
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

        float[,] dirtHeights = _noiseService.Generate(pers, freq, amp, octaves, rand.Next(), sizex, sizey);

        float minRoadPercent = 0.20f;

		float maxOtherPercent = 0.80f;

        amp = MathUtils.FloatRange(0.6f, 1.3f, rand)*globalScale;
        freq = MathUtils.FloatRange(0.15f, 0.25f, rand) * size * globalScale;
        pers = MathUtils.FloatRange(0.4f, 0.7f, rand) * globalScale;
        octaves = 2;
        float[,] baseHeights = _noiseService.Generate(pers, freq, amp, octaves, rand.Next(), sizex, sizey);


        float startMaxPct = 0.85f;

        float pctamp = MathUtils.FloatRange(0.2f,0.3f, rand)* globalScale;
        float pctfreq = MathUtils.FloatRange(0.1f, 0.2f, rand) * size * globalScale;
        float pctpers = MathUtils.FloatRange(0.0f, 0.4f, rand) * globalScale;
        int pctoctaves = 2;
        float[,] maxPcts = _noiseService.Generate(pctpers, pctfreq, pctamp, pctoctaves, rand.Next(), sizex, sizey);


        float generalPerturb = 0.1f;

        for (int x = minx; x <= maxx; x++)
		{
            if (x < 0 || x >= _mapProvider.GetMap().GetHwid())
            {
                continue;
            }

			for (int z = miny; z <= maxy; z++)
			{
                if (z < 0 || z >= _mapProvider.GetMap().GetHhgt())
                {
                    continue;
                }

				if (_md.alphas[x,z,MapConstants.RoadTerrainIndex] < minRoadPercent)
                {
                    continue;
                }

                if (_md.mapZoneIds[x,z] != zone.IdKey)
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

				_md.ClearAlphasAt(x,z);
				_md.alphas[x,z,MapConstants.RoadTerrainIndex] = 1-totalPct;
				_md.alphas[x,z,MapConstants.DirtTerrainIndex] = dirtPct;
				_md.alphas[x,z,MapConstants.BaseTerrainIndex] = basePct;

			}
		}
	}
}