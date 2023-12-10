using Cysharp.Threading.Tasks;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using System.Threading;

public class ProxyCharacterObjectLoader : UnitObjectLoader
{
    public ProxyCharacterObjectLoader(UnityGameState gs) : base(gs)
    {

    }
    public override long GetKey()
    {
        return EntityTypes.ProxyCharacter;
    }

    public override async UniTask Load(UnityGameState gs, OnSpawn spawn, MapObject obj, CancellationToken token)
    {
        await base.Load(gs, spawn, obj, token);
    }
    protected override void AfterLoadUnit(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        base.AfterLoadUnit(gs, url, obj, data, token);
    }
}
