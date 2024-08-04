using Genrpg.Shared.Movement.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.MessageHandlers.Movement
{
    public class OnAddToGridHandler : BaseClientMapMessageHandler<OnAddToGrid>
    {
        protected override void InnerProcess(OnAddToGrid msg, CancellationToken token)
        {
            _objectManager.OnServerAddtoGrid(msg);
        }
    }
}
