using Cysharp.Threading.Tasks;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using System.Threading;

public class ProxyCharacterObjectLoader : UnitObjectLoader
{
    public override long GetKey()
    {
        return EntityTypes.ProxyCharacter;
    }

    public override async UniTask Load(UnityGameState gs, OnSpawn spawn, MapObject obj, CancellationToken token)
    {
        await base.Load(gs, spawn, obj, token);
    }
    protected override void AfterLoadUnit(UnityGameState gs, object obj, object data, CancellationToken token)
    {
        base.AfterLoadUnit(gs, obj, data, token);
    }
}
