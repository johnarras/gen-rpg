using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Characters.Utils;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages.CreateChar;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Messages.DeleteChar;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.RequestServer.Core;

namespace Genrpg.RequestServer.ClientCommands
{
    public class DeleteCharHandler : BaseClientCommandHandler<DeleteCharCommand>
    {
        protected override async Task InnerHandleMessage(WebContext context, DeleteCharCommand command, CancellationToken token)
        {
            CoreCharacter coreCh = await _repoService.Load<CoreCharacter>(command.CharId);

            if (coreCh != null && coreCh.UserId == context.user.Id)
            {
                Character ch = new Character(_repoService);
                CharacterUtils.CopyDataFromTo(coreCh, ch);

                await _playerDataService.LoadAllPlayerData(context.rand, context.user, ch);
                await _repoService.Delete(coreCh);

                foreach (IUnitData data in ch.GetAllData().Values)
                {
                    if (data.Id != context.user.Id) // Do not delete user data
                    {
                        data.QueueDelete(_repoService);
                    }
                }
                coreCh = null;
            }

            DeleteCharResult result = new DeleteCharResult()
            {
                AllCharacters = await _playerDataService.LoadCharacterStubs(context.user.Id),
            };

            context.Results.Add(result);
        }
    }
}

