using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using System.Threading;

public class SpawnLoadData
{
    public MapObject Obj;
    public OnSpawn Spawn;
    public bool FixedPosition;
    public CancellationToken Token;
}