
using Cysharp.Threading.Tasks;

using System.Threading;

public class CreateConnectedZones : BaseAddMountains
{
    protected IMapGenService _mapGenService;
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await UniTask.CompletedTask;

        _mapGenService.CreateZones(gs);

        float zonesDesired = gs.map.BlockCount / gs.map.ZoneSize;

        zonesDesired *= zonesDesired;

        if (zonesDesired < 1)
        {
            gs.logger.Error("FAILED TO GENERATE ENOUGH ZONES");
            return;
        }


        return;


    }
}