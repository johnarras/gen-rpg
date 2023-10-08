using System.Threading.Tasks;
using Genrpg.Shared.Combat.Messages;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.Units.Entities;
using System.Threading;

public class OnSpawnHandler : BaseClientMapMessageHandler<OnSpawn>
{
    protected override void InnerProcess(UnityGameState gs, OnSpawn spawnMessage, CancellationToken token)
    {
        if (_objectManager.GetObject(spawnMessage.MapObjectId, out MapObject obj))
        {
            if (obj is Unit existingUnit)
            {
                existingUnit.Flags = spawnMessage.TempFlags;
                existingUnit.Stats = spawnMessage.Stats;
                existingUnit.Loot = spawnMessage.Loot;
                existingUnit.SkillLoot = spawnMessage.SkillLoot;

                if (existingUnit.HasFlag(UnitFlags.IsDead))
                {
                    if (_objectManager.GetController(spawnMessage.MapObjectId, out UnitController controller))
                    {
                        Died died = new Died()
                        {
                            UnitId = spawnMessage.MapObjectId,
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

            IMapObjectLoader loader = _terrainManager.GetMapObjectLoader(spawnMessage.EntityTypeId);

            if (loader != null)
            {
                TaskUtils.AddTask(loader.Load(gs, spawnMessage, newObj, token));
            }
        }
    }
}
