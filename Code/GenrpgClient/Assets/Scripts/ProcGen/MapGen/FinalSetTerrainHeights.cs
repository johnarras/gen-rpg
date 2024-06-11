
using Genrpg.Shared.Utils;
using System.Threading;
using UnityEngine;

public class SetfinalTerrainHeights : BaseZoneGenerator
{
    public override async Awaitable Generate (CancellationToken token)
    {
        await base.Generate(token);
        for (int x = 0; x < _mapProvider.GetMap().GetHwid(); x++)
        {
            for (int y = 0; y < _mapProvider.GetMap().GetHhgt(); y++)
            {
                if (x == 0 || x == _mapProvider.GetMap().GetHwid()-1 || y == 0 || y == _mapProvider.GetMap().GetHhgt()-1)
                {
                    _md.heights[x, y] = 0;
                }
                _md.heights[x, y] = MathUtils.Clamp(0, _md.heights[x, y], 1);
            }
        }

        _zoneGenService.SetAllHeightmaps(_md.heights, token);


		_md.HaveSetHeights = true;
	}
}
	
