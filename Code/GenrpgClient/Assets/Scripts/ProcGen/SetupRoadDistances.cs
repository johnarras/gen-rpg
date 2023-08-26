
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;


using Genrpg.Shared.Core.Entities;


using Services;
using Cysharp.Threading.Tasks;
using Entities;
using Genrpg.Shared.MapServer.Entities;
using System.Threading;

public class SetupRoadDistances : BaseZoneGenerator
{
    public override async UniTask Generate (UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        gs.md.roadDistances = new float[gs.md.awid,gs.md.ahgt];

		for (int x = 0; x < gs.md.awid; x++)
		{
			for (int y = 0; y < gs.md.ahgt; y++)
			{
                gs.md.roadDistances[x, y] = MapConstants.InitialRoadDistance;
			}
		}

		for (int x = 0; x < gs.md.awid; x++)
		{
			for (int y = 0; y < gs.md.ahgt; y++)
			{
				if (gs.md.alphas[x,y,MapConstants.RoadTerrainIndex] == 0)
				{
					continue;
				}

                if (gs.md.alphas[x, y, MapConstants.RoadTerrainIndex] >= 0.5f)
                {
                    gs.md.overrideZonePercents[x, y] = 0;
                }
				for (int xx = x- MapConstants.MaxRoadCheckDistance; xx <= x+ MapConstants.MaxRoadCheckDistance; xx++)
				{
					if (xx < 0 || xx >= gs.md.awid)
					{
						continue;
					}
					for (int yy = y- MapConstants.MaxRoadCheckDistance; yy <= y+ MapConstants.MaxRoadCheckDistance; yy++)
					{
						if (yy < 0 || yy >= gs.md.ahgt)
						{
							continue;
						}

						double dist = (ushort)Math.Sqrt ((xx-x)*(xx-x)+(yy-y)*(yy-y));
						if (dist < gs.md.roadDistances[xx,yy])
						{
                            gs.md.roadDistances[xx, yy] = (float)dist;
						}
					}
				}
			}
		}
	}
}
	
