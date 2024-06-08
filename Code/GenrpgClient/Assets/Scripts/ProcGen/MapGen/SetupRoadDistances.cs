
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using GEntity = UnityEngine.GameObject;


using Genrpg.Shared.Core.Entities;



using Cysharp.Threading.Tasks;

using Genrpg.Shared.MapServer.Entities;
using System.Threading;

public class SetupRoadDistances : BaseZoneGenerator
{
    public override async UniTask Generate (CancellationToken token)
    {
        await base.Generate(token);
        _md.roadDistances = new float[_md.awid,_md.ahgt];

		for (int x = 0; x < _md.awid; x++)
		{
			for (int y = 0; y < _md.ahgt; y++)
			{
                _md.roadDistances[x, y] = MapConstants.InitialRoadDistance;
			}
		}

		for (int x = 0; x < _md.awid; x++)
		{
			for (int y = 0; y < _md.ahgt; y++)
			{
				if (_md.alphas[x,y,MapConstants.RoadTerrainIndex] == 0)
				{
					continue;
				}

                if (_md.alphas[x, y, MapConstants.RoadTerrainIndex] >= 0.5f)
                {
                    _md.subZonePercents[x, y] = 0;
                }
				for (int xx = x- MapConstants.MaxRoadCheckDistance; xx <= x+ MapConstants.MaxRoadCheckDistance; xx++)
				{
					if (xx < 0 || xx >= _md.awid)
					{
						continue;
					}
					for (int yy = y- MapConstants.MaxRoadCheckDistance; yy <= y+ MapConstants.MaxRoadCheckDistance; yy++)
					{
						if (yy < 0 || yy >= _md.ahgt)
						{
							continue;
						}

						double dist = (ushort)Math.Sqrt ((xx-x)*(xx-x)+(yy-y)*(yy-y));
						if (dist < _md.roadDistances[xx,yy])
						{
                            _md.roadDistances[xx, yy] = (float)dist;
						}
					}
				}
			}
		}
	}
}
	
