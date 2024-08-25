
using Genrpg.ServerShared.Purchasing.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Website.Messages.RefreshStores;
using Genrpg.RequestServer.Core;

namespace Genrpg.RequestServer.ClientCommands
{
    public class RefreshStoresHandler : BaseClientCommandHandler<RefreshStoresCommand>
    {
        private IPurchasingService _purchasingService = null;

        protected override async Task InnerHandleMessage(WebContext context, RefreshStoresCommand command, CancellationToken token)
        {
            ICoreCharacter coreCh = await _repoService.Load<CoreCharacter>(command.CharId);
            Character ch = new Character(_repoService);

            PlayerStoreOfferData offerData = await _purchasingService.GetCurrentStores(context.user, true);

            RefreshStoresResult result = new RefreshStoresResult();

            result.Stores = offerData;

            if (result != null)
            {
                context.Results.Add(result);
            }

        }
    }
}
