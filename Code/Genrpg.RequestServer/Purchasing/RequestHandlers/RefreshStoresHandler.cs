using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Purchasing.Services;
using Genrpg.RequestServer.ClientUser.RequestHandlers;
using Genrpg.Shared.Purchasing.WebApi.RefreshStores;

namespace Genrpg.RequestServer.Purchasing.RequestHandlers
{
    public class RefreshStoresHandler : BaseClientUserRequestHandler<RefreshStoresRequest>
    {
        private IServerPurchasingService _purchasingService = null;

        protected override async Task InnerHandleMessage(WebContext context, RefreshStoresRequest request, CancellationToken token)
        {
            ICoreCharacter coreCh = await _repoService.Load<CoreCharacter>(request.CharId);
            Character ch = new Character(_repoService);

            PlayerStoreOfferData offerData = await _purchasingService.GetCurrentStores(context.user, true);

            RefreshStoresResponse response = new RefreshStoresResponse();

            response.Stores = offerData;

            if (response != null)
            {
                context.Responses.Add(response);
            }

        }
    }
}
