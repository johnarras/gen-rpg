using Genrpg.LoginServer.Core;
using Genrpg.MonsterServer.MessageHandlers;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Input.Entities;
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
    public class CreateCharHandler : BaseLoginCommandHandler<CreateCharCommand>
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

            gs.ch = new Character()
            {
                Id = gs.user.Id + "." + nextId,
                Name = command.Name,
                UserId = gs.user.Id,
            };

            await SetupCharacter(gs);

            charStubs.Add(new CharacterStub() { Id = gs.ch.Id, Name = gs.ch.Name, Level = gs.ch.Level });

            CreateCharResult result = new CreateCharResult()
            {
                NewChar = SerializationUtils.ConvertType<Character, Character>(gs.ch),
                AllCharacters = charStubs,
            };

            gs.Results.Add(result);
        }


        public async Task SetupCharacter(LoginGameState gs)
        {
            List<IUnitData> list = await _playerDataService.LoadPlayerData(gs, gs.ch);
            KeyCommData keyCommands = gs.ch.Get<KeyCommData>();
            ActionInputData actionInputs = gs.ch.Get<ActionInputData>();

            for (int i = InputConstants.MinActionIndex; i <= InputConstants.MaxActionIndex; i++)
            {
                actionInputs.GetInput(i);
            }

            if (gs.data.GetGameData<KeyCommSettings>(gs.ch).GetData() != null)
            {
                foreach (KeyCommSetting input in gs.data.GetGameData<KeyCommSettings>(gs.ch).GetData())
                {
                    KeyComm currKey = keyCommands.GetKeyComm(input.KeyCommand);
                    if (currKey == null)
                    {
                        keyCommands.AddKeyComm(input.KeyCommand, input.KeyPress);
                    }
                    if (input.KeyCommand.IndexOf(KeyComm.ActionPrefix) == 0)
                    {
                        string actionSuffix = input.KeyCommand.Replace(KeyComm.ActionPrefix, "");
                        int actionIndex = -1;

                        int.TryParse(actionSuffix, out actionIndex);

                        ActionInput currAction = actionInputs.GetInput(actionIndex);
                        if (gs.data.GetGameData<InputSettings>(gs.ch).GetData() != null)
                        {
                            ActionInputSetting defaultAction = gs.data.GetGameData<InputSettings>(gs.ch).GetData().FirstOrDefault(x => x.Index == actionIndex);
                            if (defaultAction != null)
                            {
                                currAction.SpellId = defaultAction.SpellId;
                            }
                        }
                    }
                }
            }
        }
    }
}
