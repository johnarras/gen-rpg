using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Login.Messages.RefreshGameSettings;
using Genrpg.Shared.Login.Messages.RefreshStores;
using System.Threading;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class RefreshStoresResultHandler : BaseClientLoginResultHandler<RefreshStoresResult>
    {
        protected override void InnerProcess(RefreshStoresResult result, CancellationToken token)
        {
            result.Stores?.AddTo(_gs.ch);
            _dispatcher.Dispatch(result);
        }
    }
}
