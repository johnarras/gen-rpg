using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Achievements.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Characters.Utils;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Input.Constants;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.Input.Settings;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Messages.CreateChar;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Entities.Constants;
using Genrpg.RequestServer.Core;

namespace Genrpg.RequestServer.ClientCommands
{
    public class CreateCharHandler : BaseClientCommandHandler<CreateCharCommand>
    {
        protected override async Task InnerHandleMessage(WebContext context, CreateCharCommand command, CancellationToken token)
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

            CoreCharacter coreCh = new CoreCharacter()
            {
                Id = context.user.Id + "." + nextId,
                Name = command.Name,
                UserId = context.user.Id,
                Level = 1,
                EntityTypeId = EntityTypes.Unit,
                EntityId = command.UnitTypeId,
                SexTypeId = command.SexTypeId,
            };
            Character ch = new Character(_repoService);
            CharacterUtils.CopyDataFromTo(coreCh, ch);
            await _repoService.Save(coreCh);

            charStubs.Add(new CharacterStub() { Id = coreCh.Id, Name = coreCh.Name, Level = coreCh.Level });

            CreateCharResult result = new CreateCharResult()
            {
                NewChar = SerializationUtils.ConvertType<Character, Character>(ch),
                AllCharacters = charStubs,
            };

            context.Results.Add(result);

        }
    }
}
