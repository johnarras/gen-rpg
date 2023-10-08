
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using GEntity = UnityEngine.GameObject;


using Genrpg.Shared.Core.Entities;



using System.Threading.Tasks;

using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Entities;

using System.Threading;

public class SmoothRoadEdges : BaseZoneGenerator
{
    public override async Task Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);

		int awid = gs.md.awid;
		int ahgt = gs.md.ahgt;
		int hwid = gs.map.GetHwid();
		int hhgt = gs.map.GetHhgt();
		
		float[,] heights2 = new float[gs.map.GetHwid(),gs.map.GetHhgt()];

        int radius = 7;

        radius = radius * 3 / 2;

		for (int x = 0; x < hwid; x++)
		{
			for (int y = 0; y < hhgt; y++)
			{
				heights2[x,y] = gs.md.heights[x,y];
			}
		}
		for (int x = 0; x < hwid; x++)
		{
			for (int y = 0; y < hhgt; y++)
            {
                int ax = (int)(1.0f*x/hwid*awid);
				int ay = (int)(1.0f*y/hhgt*ahgt);


				float currSplat = gs.md.alphas[ax,ay,MapConstants.RoadTerrainIndex];
				if (currSplat > 0.0f)
				{
					//continue;
				}

				if (gs.md.roadDistances[ax,ay] >= radius)
				{
					continue;
				}

				float aveSplat = gs.md.GetAverageSplatNear(gs, ax,ay,radius,MapConstants.RoadTerrainIndex);

				int averad = radius;
				if (currSplat > 0)
				{
					averad = 2;
				}
				float aveHeight = gs.md.GetAverageHeightNear(gs, gs.map, x,y,averad);
			


				if (currSplat <= 0 && _zoneGenService.FindMapLocation(gs,x,y,5) != null)
				{
					continue;
				}


				float alterPercent = Math.Max (0.1f,1-aveSplat*3.0f);


				alterPercent = Math.Min (1.0f,aveSplat*5.0f);

				
				if (averad < 0)
				{
					alterPercent /= 3;
				}
				if (currSplat > 0)
				{
					alterPercent = 1.0f;
				}


                float bridgeDist = gs.md.bridgeDistances[y, x];

                float bridgeScale = 1.0f;

                int minBridgeDist = 15;
                if (bridgeDist < minBridgeDist)
                {
                    bridgeScale = 0.5f + bridgeDist / (2 * minBridgeDist);
                    alterPercent *= bridgeScale;
                }



                if (alterPercent <= 0)
				{
					continue;
				}

				float currHeight = gs.md.heights[x,y];

				if (aveHeight < currHeight)
				{
					aveHeight += (currHeight-aveHeight)/2;
				}

				heights2[x,y] = currHeight+(aveHeight-currHeight)*alterPercent;

			}
		}

		gs.md.heights = heights2;
	}
}
	
