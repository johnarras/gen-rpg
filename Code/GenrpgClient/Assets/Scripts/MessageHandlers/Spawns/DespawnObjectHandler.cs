
using Genrpg.Shared.MapObjects.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Spawns
{
    public class DespawnObjectHandler : BaseClientMapMessageHandler<DespawnObject>
    {
        protected override void InnerProcess(DespawnObject msg, CancellationToken token)
        {
            _objectManager.RemoveObject(msg.ObjId);
        }
    }
}
