
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.WorldData;
using System.Threading;

public class GenerateMap : BaseZoneGenerator
{
    protected IMapGenService _mapGenService;
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        gs.spawns = new MapSpawnData() { Id = gs.map.Id.ToString() };
        gs.map = _mapGenService.GenerateMap(gs, gs.map);
        
    }
}
