
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using System.Threading;
using UnityEngine;

public class ProxyCharacterObjectLoader : UnitObjectLoader
{
    public override long GetKey()
    {
        return EntityTypes.ProxyCharacter;
    }

    public override async Awaitable Load(OnSpawn spawn, MapObject obj, CancellationToken token)
    {
        await base.Load(spawn, obj, token);
    }
    protected override void AfterLoadUnit(object obj, object data, CancellationToken token)
    {
        base.AfterLoadUnit(obj, data, token);
    }
}
