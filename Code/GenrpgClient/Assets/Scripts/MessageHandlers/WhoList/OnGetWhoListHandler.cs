using Genrpg.Shared.WhoList.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.MessageHandlers.WhoList
{
    public class OnGetWhoListHandler : BaseClientMapMessageHandler<OnGetWhoList>
    {
        protected override void InnerProcess(UnityGameState gs, OnGetWhoList msg, CancellationToken token)
        {
            int count = msg.Items?.Count ?? 0;

            gs.Dispatch(msg);
        }
    }
}
