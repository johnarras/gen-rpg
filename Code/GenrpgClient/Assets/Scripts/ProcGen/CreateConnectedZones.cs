
using System.Threading.Tasks;

using System.Threading;

public class CreateConnectedZones : BaseAddMountains
{
    protected IMapGenService _mapGenService;
    public override async Task Generate(UnityGameState gs, CancellationToken token)
    {
        await Task.CompletedTask;

        _mapGenService.CreateZones(gs);

        float zonesDesired = gs.map.BlockCount / gs.map.ZoneSize;

        zonesDesired *= zonesDesired;

        if (zonesDesired < 1)
        {
            UnityZoneGenService.GenErrorMsg = "Failed to Generate enough zones!";
            return;
        }


        return;


    }
}