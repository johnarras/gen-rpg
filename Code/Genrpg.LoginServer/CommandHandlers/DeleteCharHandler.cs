using Genrpg.LoginServer.CommandHandlers.Core;
using Genrpg.LoginServer.Core;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Characters.Utils;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Login.Messages.CreateChar;
using Genrpg.Shared.Login.Messages.DeleteChar;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.CommandHandlers
{
    public class DeleteCharHandler : BaseLoginCommandHandler<DeleteCharCommand>
    {
        protected override async Task InnerHandleMessage(LoginGameState gs, DeleteCharCommand command, CancellationToken token)
        {
            gs.coreCh = await gs.repo.Load<CoreCharacter>(command.CharId);

            if (gs.coreCh != null && gs.coreCh.UserId == gs.user.Id)
            {
                Character ch = new Character();
                CharacterUtils.CopyDataFromTo(gs.coreCh, ch);

                await _playerDataService.LoadPlayerData(gs, gs.ch);
                await gs.repo.Delete(gs.coreCh);

                foreach (IUnitData data in gs.ch.GetAllData().Values)
                {
                    if (data.Id != gs.user.Id) // Do not delete user data
                    {
                        data.Delete(gs.repo);
                    }
                }
                gs.coreCh = null;
            }

            DeleteCharResult result = new DeleteCharResult()
            {
                AllCharacters = await _playerDataService.LoadCharacterStubs(gs, gs.user.Id),
            };

            gs.Results.Add(result);
        }
    }
}

