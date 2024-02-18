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

        protected override async Task InnerHandleMessage(LoginGameState gs, RefreshStoresCommand command, CancellationToken token)
        {

            gs.coreCh = await gs.repo.Load<CoreCharacter>(command.CharId);
            gs.ch = new Character();
            CharacterUtils.CopyDataFromTo(gs.coreCh, gs.ch);

            PlayerStoreOfferData offerData = await _purchasingService.GetCurrentStores(gs, gs.user, gs.ch, true);

            RefreshStoresResult result = new RefreshStoresResult();

            result.Stores = offerData;

            if (result != null)
            {
                gs.Results.Add(result);
            }

        }
    }
}
