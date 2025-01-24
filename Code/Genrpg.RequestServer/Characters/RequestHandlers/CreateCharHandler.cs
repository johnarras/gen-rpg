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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Entities.Constants;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.Characters.WebApi.CreateChar;
using Genrpg.RequestServer.ClientUser.RequestHandlers;

namespace Genrpg.RequestServer.Characters.RequestHandlers
{
    public class CreateCharHandler : BaseClientUserRequestHandler<CreateCharRequest>
    {
        protected override async Task InnerHandleMessage(WebContext context, CreateCharRequest request, CancellationToken token)
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
                Name = request.Name,
                UserId = context.user.Id,
                Level = 1,
                EntityTypeId = EntityTypes.Unit,
                EntityId = request.UnitTypeId,
                SexTypeId = request.SexTypeId,
            };
            Character ch = new Character(_repoService);
            CharacterUtils.CopyDataFromTo(coreCh, ch);
            await _repoService.Save(coreCh);

            charStubs.Add(new CharacterStub() { Id = coreCh.Id, Name = coreCh.Name, Level = coreCh.Level });

            CreateCharResponse response = new CreateCharResponse()
            {
                NewChar = SerializationUtils.ConvertType<Character, Character>(ch),
                AllCharacters = charStubs,
            };

            context.Responses.Add(response);

        }
    }
}
