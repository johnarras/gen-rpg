using Genrpg.RequestServer.BoardGame.Services;
using Genrpg.RequestServer.ClientUser.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.WebApi.RollDice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.BoardGame.Requests
{
    public class RollDiceRequestHandlers : BaseClientUserRequestHandler<RollDiceRequest>
    {
        IDiceRollService _diceRollService = null!;
        protected override async Task InnerHandleMessage(WebContext context, RollDiceRequest command, CancellationToken token)
        {
            await _diceRollService.RollDice(context, null);
        }
    }
}
