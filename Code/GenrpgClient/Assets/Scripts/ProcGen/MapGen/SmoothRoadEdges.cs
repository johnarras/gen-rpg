using System;
using Cysharp.Threading.Tasks;
using System.Threading;

public class SmoothRoadEdges : BaseZoneGenerator
{
    public override async UniTask Generate(CancellationToken token)
    {

        await base.Generate(token);

		int awid = _md.awid;
		int ahgt = _md.ahgt;
		int hwid = _mapProvider.GetMap().GetHwid();
		int hhgt = _mapProvider.GetMap().GetHhgt();
		
		float[,] heights2 = new float[_mapProvider.GetMap().GetHwid(),_mapProvider.GetMap().GetHhgt()];

        int radius = 7;

        radius = radius * 3 / 2;

		for (int x = 0; x < hwid; x++)
		{
			for (int y = 0; y < hhgt; y++)
			{
				heights2[x,y] = _md.heights[x,y];
			}
		}
		for (int x = 0; x < hwid; x++)
		{
			for (int y = 0; y < hhgt; y++)
            {
                int ax = (int)(1.0f*x/hwid*awid);
				int ay = (int)(1.0f*y/hhgt*ahgt);


				float currSplat = _md.alphas[ax,ay,MapConstants.RoadTerrainIndex];
				if (currSplat > 0.0f)
				{
					//continue;
				}

				if (_md.roadDistances[ax,ay] >= radius)
				{
					continue;
				}

				float aveSplat = _md.GetAverageSplatNear(ax,ay,radius,MapConstants.RoadTerrainIndex);

				int averad = radius;
				if (currSplat > 0)
				{
					averad = 2;
				}
				float aveHeight = _md.GetAverageHeightNear(_mapProvider.GetMap(), x,y,averad);
			


				if (currSplat <= 0 && _zoneGenService.FindMapLocation(x,y,5) != null)
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


                float bridgeDist = _md.bridgeDistances[y, x];

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

				float currHeight = _md.heights[x,y];

				if (aveHeight < currHeight)
				{
					aveHeight += (currHeight-aveHeight)/2;
				}

				heights2[x,y] = currHeight+(aveHeight-currHeight)*alterPercent;

			}
		}

		_md.heights = heights2;
	}
}
	
