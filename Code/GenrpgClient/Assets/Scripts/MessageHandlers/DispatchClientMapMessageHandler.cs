using Genrpg.Shared.MapMessages.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.MessageHandlers
{
    public abstract class DispatchClientMapMessageHandler<T> : BaseClientMapMessageHandler<T> where T : class, IMapApiMessage
    {
        protected override void InnerProcess(UnityGameState gs, T msg, CancellationToken token)
        {
            _dispatcher.Dispatch(msg);
        }
    }
}
