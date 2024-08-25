using Genrpg.RequestServer.BoardGame.Services;
using Genrpg.RequestServer.ClientCommands;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Messages.RollDice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.BoardGame.Commands
{
    public class RollDiceCommandHandler : BaseClientCommandHandler<RollDiceCommand>
    {
        IDiceRollService _diceRollService = null!;
        protected override async Task InnerHandleMessage(WebContext context, RollDiceCommand command, CancellationToken token)
        {
            await _diceRollService.RollDice(context, null);
        }
    }
}
