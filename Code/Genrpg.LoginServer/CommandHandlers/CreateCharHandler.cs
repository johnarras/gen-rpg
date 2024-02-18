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
        protected override async Task InnerHandleMessage(LoginGameState gs, CreateCharCommand command, CancellationToken token)
        {
            List<CharacterStub> charStubs = await _playerDataService.LoadCharacterStubs(gs, gs.user.Id);

            int nextId = 1;

            while (true)
            {
                if (charStubs.FirstOrDefault(x => x.Id == gs.user.Id + "." + nextId) == null)
                {
                    break;
                }
                nextId++;
            }

            gs.coreCh = new CoreCharacter()
            {
                Id = gs.user.Id + "." + nextId,
                Name = command.Name,
                UserId = gs.user.Id,
            };
            gs.ch = new Character();
            CharacterUtils.CopyDataFromTo(gs.coreCh, gs.ch);


            List<IUnitData> list = await _playerDataService.LoadPlayerData(gs, gs.ch);

            charStubs.Add(new CharacterStub() { Id = gs.coreCh.Id, Name = gs.coreCh.Name, Level = gs.coreCh.Level });

            CreateCharResult result = new CreateCharResult()
            {
                NewChar = SerializationUtils.ConvertType<Character, Character>(gs.ch),
                AllCharacters = charStubs,
            };

            gs.Results.Add(result);
        }
    }
}
