using System.Threading.Tasks;
using Genrpg.Shared.Utils;
using System.Threading;

public class SetfinalTerrainHeights : BaseZoneGenerator
{
    public override async Task Generate (UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        for (int x = 0; x < gs.map.GetHwid(); x++)
        {
            for (int y = 0; y < gs.map.GetHhgt(); y++)
            {
                if (x == 0 || x == gs.map.GetHwid()-1 || y == 0 || y == gs.map.GetHhgt()-1)
                {
                    gs.md.heights[x, y] = 0;
                }
                gs.md.heights[x, y] = MathUtils.Clamp(0, gs.md.heights[x, y], 1);
            }
        }

        _zoneGenService.SetAllHeightmaps(gs, gs.md.heights, token);


		gs.md.HaveSetHeights = true;
	}
}
	
