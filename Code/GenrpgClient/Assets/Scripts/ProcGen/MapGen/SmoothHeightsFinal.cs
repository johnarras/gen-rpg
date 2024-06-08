
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

public class SmoothHeightsFinal : BaseZoneGenerator
{
    public override async UniTask Generate(CancellationToken token)
    {
        await base.Generate(token);
        int hwid = _mapProvider.GetMap().GetHwid();
		int hhgt = _mapProvider.GetMap().GetHhgt();

		float[,] heights2 = new float[_mapProvider.GetMap().GetHwid(),_mapProvider.GetMap().GetHhgt()];


        int minRadius = 2;
        float smoothScale = 0.5f;
        int checkRadius = 8;


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
                int currRadius = minRadius;
                int numRoadCellsChecked = 0;
                float totalRoadPercent = 0;
                float currSmoothingScale = smoothScale;
                float otherZoneDist = 10000;


                for (int xx = x - checkRadius; xx <= x + checkRadius; xx++)
                {
                    if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
                    {
                        continue;
                    }

                    for (int yy = y - checkRadius; yy <= y + checkRadius; yy++)
                    {
                        if (yy < 0 || yy >= _mapProvider.GetMap().GetHhgt())
                        {
                            continue;
                        }
                        numRoadCellsChecked++;
                        totalRoadPercent += _md.alphas[xx, yy, MapConstants.RoadTerrainIndex];
			            if (_md.mapZoneIds[xx,yy] != _md.mapZoneIds[x,y])
                        {
                            float dx = xx - x;
                            float dy = yy - y;

                            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                            if (dist < otherZoneDist)
                            {
                                otherZoneDist = dist;
                            }
                        }
                    }
                }

                float bridgeDist = 100;

                if (_md.bridgeDistances != null)
                {
                    bridgeDist = _md.bridgeDistances[x,y];
                }

                float bridgeScale = 1.0f;

                int minBridgeDist = 40;
                if (bridgeDist < minBridgeDist)
                {
                    float mainPct = 0.2f;
                    bridgeScale = mainPct + (1-mainPct)*bridgeDist / (1.0f*minBridgeDist);
                    currSmoothingScale *= bridgeScale;
                }


                if (totalRoadPercent > 0 && false)
                {
                    float adjustedRoadPercent = Math.Min(1, 1.5f * totalRoadPercent / numRoadCellsChecked);
                    currSmoothingScale *= (1 - adjustedRoadPercent);
                }

                else if (otherZoneDist < checkRadius)
                {
                    float newSmoothingScale = (checkRadius - otherZoneDist + 1) / checkRadius;
                    if (newSmoothingScale > 1)
                    {
                        newSmoothingScale = 1;
                    }

                    if (newSmoothingScale > currSmoothingScale)
                    {
                        currSmoothingScale = newSmoothingScale;
                    }
                }


                float totalWeight = 0;
				float totalVal = 0;


				for (int xx = x-currRadius; xx <= x+currRadius; xx++)
				{
					if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
					{
						continue;
					}

					float dx = Math.Abs (xx-x);
					for (int yy = y-currRadius; yy <= y+currRadius; yy++)
					{
						if (yy < 0 || yy >= _mapProvider.GetMap().GetHhgt())
						{
							continue;
						}


                        float dy = Math.Abs (yy-y);

						float totalOffset = dx+dy;

						float currweight = 1;
						currweight = (float)Math.Pow(currSmoothingScale,totalOffset);
						



						totalVal += _md.heights[xx,yy]*currweight;
						totalWeight += currweight;
					
					}
				}

				if (totalWeight <= 0)
				{
					continue;
				}

				heights2[x,y] = totalVal/totalWeight;
			}

		}
		_md.heights = heights2;
	}
}
	
