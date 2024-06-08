using Genrpg.Shared.Spells.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Spells
{
    public class OnStartCastHandler : BaseClientMapMessageHandler<OnStartCast>
    {
        protected override void InnerProcess(OnStartCast msg, CancellationToken token)
        {
            if (_objectManager.GetGridItem(msg.CasterId, out ClientMapObjectGridItem gridItem))
            {
                gridItem.Controller?.StartCasting(msg);
            }
        }
    }
}
