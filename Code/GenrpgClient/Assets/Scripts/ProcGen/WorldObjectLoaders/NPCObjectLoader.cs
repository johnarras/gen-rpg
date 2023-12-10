using Cysharp.Threading.Tasks;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using System.Threading;

public class NPCObjectLoader : UnitObjectLoader
{
    public NPCObjectLoader(UnityGameState gs) : base(gs)
    {

    }
    public override long GetKey() { return EntityTypes.NPC; }

    public override async UniTask Load(UnityGameState gs, OnSpawn spawn, MapObject obj, CancellationToken token)
    {
        await base.Load(gs, spawn, obj, token);
    }
}
