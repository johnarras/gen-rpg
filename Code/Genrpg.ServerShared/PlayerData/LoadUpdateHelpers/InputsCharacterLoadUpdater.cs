using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Input.Constants;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.Input.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.PlayerData.LoadUpdateHelpers
{
    public class InputsCharacterLoadUpdater : BaseCharacterLoadUpdater
    {
        private IGameData _gameData;
        public override async Task Update(GameState gs, Character ch)
        {
            KeyCommData keyCommands = ch.Get<KeyCommData>();
            ActionInputData actionInputs = ch.Get<ActionInputData>();

            for (int i = InputConstants.MinActionIndex; i <= InputConstants.MaxActionIndex; i++)
            {
                actionInputs.GetInput(i);
            }

            if (_gameData.Get<KeyCommSettings>(ch).GetData() != null)
            {
                foreach (KeyCommSetting input in _gameData.Get<KeyCommSettings>(ch).GetData())
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
                        if (_gameData.Get<InputSettings>(ch).GetData() != null)
                        {
                            ActionInputSetting defaultAction = _gameData.Get<InputSettings>(ch).GetData().FirstOrDefault(x => x.Index == actionIndex);
                            if (defaultAction != null)
                            {
                                currAction.SpellId = defaultAction.SpellId;
                            }
                        }
                    }
                }
            }

            await Task.CompletedTask;
        }
    }
}
