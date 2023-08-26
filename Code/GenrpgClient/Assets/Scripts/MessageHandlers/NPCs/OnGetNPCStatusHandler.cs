using Genrpg.Shared.NPCs.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.MessageHandlers.NPCs
{
    public class OnGetNPCStatusHandler : BaseClientMapMessageHandler<OnGetNPCStatus>
    {
        protected override void InnerProcess(UnityGameState gs, OnGetNPCStatus msg, CancellationToken token)
        {
            gs.Dispatch(msg);
        }
    }
}
