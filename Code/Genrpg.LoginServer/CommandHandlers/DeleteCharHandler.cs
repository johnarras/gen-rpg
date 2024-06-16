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
    public class DeleteCharHandler : BaseClientCommandHandler<DeleteCharCommand>
    {
        protected override async Task InnerHandleMessage(LoginContext context, DeleteCharCommand command, CancellationToken token)
        {
            context.coreCh = await _repoService.Load<CoreCharacter>(command.CharId);

            if (context.coreCh != null && context.coreCh.UserId == context.user.Id)
            {
                Character ch = new Character(_repoService);
                CharacterUtils.CopyDataFromTo(context.coreCh, ch);

                await _playerDataService.LoadAllPlayerData(context.rand, context.ch);
                await _repoService.Delete(context.coreCh);

                foreach (IUnitData data in context.ch.GetAllData().Values)
                {
                    if (data.Id != context.user.Id) // Do not delete user data
                    {
                        data.QueueDelete(_repoService);
                    }
                }
                context.coreCh = null;
            }

            DeleteCharResult result = new DeleteCharResult()
            {
                AllCharacters = await _playerDataService.LoadCharacterStubs(context.user.Id),
            };

            context.Results.Add(result);
        }
    }
}

