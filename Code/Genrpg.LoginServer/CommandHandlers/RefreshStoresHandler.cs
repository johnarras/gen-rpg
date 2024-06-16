using Genrpg.LoginServer.CommandHandlers.Core;
using Genrpg.LoginServer.Core;
using Genrpg.LoginServer.PlayerData;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.Purchasing.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Characters.Utils;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Login.Messages.RefreshGameSettings;
using Genrpg.Shared.Login.Messages.RefreshStores;
using Genrpg.Shared.Purchasing.PlayerData;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.CommandHandlers
{
    public class RefreshStoresHandler : BaseClientCommandHandler<RefreshStoresCommand>
    {
        private IPurchasingService _purchasingService = null;

        protected override async Task InnerHandleMessage(LoginContext context, RefreshStoresCommand command, CancellationToken token)
        {

            context.coreCh = await _repoService.Load<CoreCharacter>(command.CharId);
            context.ch = new Character(_repoService);
            CharacterUtils.CopyDataFromTo(context.coreCh, context.ch);

            PlayerStoreOfferData offerData = await _purchasingService.GetCurrentStores(context.user, context.ch, true);

            RefreshStoresResult result = new RefreshStoresResult();

            result.Stores = offerData;

            if (result != null)
            {
                context.Results.Add(result);
            }

        }
    }
}
