using Genrpg.LoginServer.CommandHandlers.Core;
using Genrpg.LoginServer.Core;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Characters.Utils;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Input.Constants;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.Input.Settings;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Login.Messages.CreateChar;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.CommandHandlers
{
    public class CreateCharHandler : BaseClientCommandHandler<CreateCharCommand>
    {
        protected override async Task InnerHandleMessage(LoginContext context, CreateCharCommand command, CancellationToken token)
        {
            List<CharacterStub> charStubs = await _playerDataService.LoadCharacterStubs(context.user.Id);

            int nextId = 1;

            while (true)
            {
                if (charStubs.FirstOrDefault(x => x.Id == context.user.Id + "." + nextId) == null)
                {
                    break;
                }
                nextId++;
            }

            context.coreCh = new CoreCharacter()
            {
                Id = context.user.Id + "." + nextId,
                Name = command.Name,
                UserId = context.user.Id,
            };
            context.ch = new Character(_repoService);
            CharacterUtils.CopyDataFromTo(context.coreCh, context.ch);


            List<IUnitData> list = await _playerDataService.LoadAllPlayerData(context.rand, context.ch);

            charStubs.Add(new CharacterStub() { Id = context.coreCh.Id, Name = context.coreCh.Name, Level = context.coreCh.Level });

            CreateCharResult result = new CreateCharResult()
            {
                NewChar = SerializationUtils.ConvertType<Character, Character>(context.ch),
                AllCharacters = charStubs,
            };

            context.Results.Add(result);
        }
    }
}
