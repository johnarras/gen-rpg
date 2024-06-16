


using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class CreateConnectedZones : BaseAddMountains
{
    protected IMapGenService _mapGenService;
    public override async Awaitable Generate(CancellationToken token)
    {
        _mapGenService.CreateZones(_gs);

        float zonesDesired = _mapProvider.GetMap().BlockCount / _mapProvider.GetMap().ZoneSize;

        zonesDesired *= zonesDesired;

        if (zonesDesired < 1)
        {
            _logService.Error("FAILED TO GENERATE ENOUGH ZONES");
            return;
        }

        await Task.CompletedTask;
        return;
    }
}