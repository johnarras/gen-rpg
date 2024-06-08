using Cysharp.Threading.Tasks;
using Genrpg.Shared.Combat.Messages;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Units.Entities;
using System.Threading;

public class OnSpawnHandler : BaseClientMapMessageHandler<OnSpawn>
{
    protected override void InnerProcess(OnSpawn spawnMessage, CancellationToken token)
    {
        if (_objectManager.GetObject(spawnMessage.ObjId, out MapObject obj))
        {
            if (obj is Unit existingUnit)
            {
                existingUnit.AddFlag(spawnMessage.TempFlags);

                existingUnit.Stats.UpdateFromSnapshot(spawnMessage.Stats);

                existingUnit.Loot = spawnMessage.Loot;
                existingUnit.SkillLoot = spawnMessage.SkillLoot;

                if (existingUnit.HasFlag(UnitFlags.IsDead))
                {
                    if (_objectManager.GetController(spawnMessage.ObjId, out UnitController controller))
                    {
                        Died died = new Died()
                        {
                            UnitId = spawnMessage.ObjId,
                        };
                        controller.OnDeath(died, token);
                    }
                }
            }
            return;
        }

        MapObject newObj = _objectManager.SpawnObject(spawnMessage);

        if (newObj != null)
        {
            _objectManager.AddObject(newObj, null);

            IMapObjectLoader loader = _objectManager.GetMapObjectLoader(spawnMessage.EntityTypeId);

            if (loader != null)
            {
                loader.Load(spawnMessage, newObj, token).Forget();
            }
        }
    }
}
