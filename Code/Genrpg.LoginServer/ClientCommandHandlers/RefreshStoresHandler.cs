using Genrpg.LoginServer.Core;
using Genrpg.LoginServer.PlayerData;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.Purchasing.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Characters.Utils;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Website.Messages.RefreshGameSettings;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Website.Messages.RefreshStores;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.ClientCommandHandlers
{
    public class RefreshStoresHandler : BaseClientCommandHandler<RefreshStoresCommand>
    {
        private IPurchasingService _purchasingService = null;

        protected override async Task InnerHandleMessage(WebContext context, RefreshStoresCommand command, CancellationToken token)
        {
            ICoreCharacter coreCh = await _repoService.Load<CoreCharacter>(command.CharId);
            Character ch = new Character(_repoService);

            PlayerStoreOfferData offerData = await _purchasingService.GetCurrentStores(context.user, coreCh, true);

            RefreshStoresResult result = new RefreshStoresResult();

            result.Stores = offerData;

            if (result != null)
            {
                context.Results.Add(result);
            }

        }
    }
}
