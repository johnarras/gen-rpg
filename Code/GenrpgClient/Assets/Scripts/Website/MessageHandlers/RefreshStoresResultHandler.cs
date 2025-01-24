using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Purchasing.WebApi.RefreshStores;
using System.Threading;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class RefreshStoresResultHandler : BaseClientLoginResultHandler<RefreshStoresResponse>
    {
        protected override void InnerProcess(RefreshStoresResponse result, CancellationToken token)
        {
            result.Stores?.AddTo(_gs.ch);
            _dispatcher.Dispatch(result);
        }
    }
}
