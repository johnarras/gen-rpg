
using System;
using UnityEngine;
using Genrpg.Shared.Core.Entities;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Interfaces;
using Entities;
using Genrpg.Shared.MapServer.Entities;
using Services.ProcGen;
using System.Threading;

public class SmoothHeightsFinal : BaseZoneGenerator
{
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        int hwid = gs.map.GetHwid();
		int hhgt = gs.map.GetHhgt();

		float[,] heights2 = new float[gs.map.GetHwid(),gs.map.GetHhgt()];


        int minRadius = 2;
        float smoothScale = 0.5f;
        int checkRadius = 8;


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
                int currRadius = minRadius;
                int numRoadCellsChecked = 0;
                float totalRoadPercent = 0;
                float currSmoothingScale = smoothScale;
                float otherZoneDist = 10000;


                for (int xx = x - checkRadius; xx <= x + checkRadius; xx++)
                {
                    if (xx < 0 || xx >= gs.map.GetHwid())
                    {
                        continue;
                    }

                    for (int yy = y - checkRadius; yy <= y + checkRadius; yy++)
                    {
                        if (yy < 0 || yy >= gs.map.GetHhgt())
                        {
                            continue;
                        }
                        numRoadCellsChecked++;
                        totalRoadPercent += gs.md.alphas[xx, yy, MapConstants.RoadTerrainIndex];
			            if (gs.md.mapZoneIds[xx,yy] != gs.md.mapZoneIds[x,y])
                        {
                            float dx = xx - x;
                            float dy = yy - y;

                            float dist = Mathf.Sqrt(dx * dx + dy * dy);
                            if (dist < otherZoneDist)
                            {
                                otherZoneDist = dist;
                            }
                        }
                    }
                }

                float bridgeDist = 100;

                if (gs.md.bridgeDistances != null)
                {
                    bridgeDist = gs.md.bridgeDistances[x,y];
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
					if (xx < 0 || xx >= gs.map.GetHwid())
					{
						continue;
					}

					float dx = Mathf.Abs (xx-x);
					for (int yy = y-currRadius; yy <= y+currRadius; yy++)
					{
						if (yy < 0 || yy >= gs.map.GetHhgt())
						{
							continue;
						}


                        float dy = Mathf.Abs (yy-y);

						float totalOffset = dx+dy;

						float currweight = 1;
						currweight = Mathf.Pow(currSmoothingScale,totalOffset);
						



						totalVal += gs.md.heights[xx,yy]*currweight;
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
		gs.md.heights = heights2;
	}
}
	
