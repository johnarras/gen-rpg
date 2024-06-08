
using Cysharp.Threading.Tasks;

using System.Threading;

public class CreateConnectedZones : BaseAddMountains
{
    protected IMapGenService _mapGenService;
    public override async UniTask Generate(CancellationToken token)
    {
        await UniTask.CompletedTask;

        _mapGenService.CreateZones(_gs);

        float zonesDesired = _mapProvider.GetMap().BlockCount / _mapProvider.GetMap().ZoneSize;

        zonesDesired *= zonesDesired;

        if (zonesDesired < 1)
        {
            _logService.Error("FAILED TO GENERATE ENOUGH ZONES");
            return;
        }


        return;


    }
}