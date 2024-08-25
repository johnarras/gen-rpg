using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.BoardGame.Helpers.BoardLoadHelpers
{
    public class DailyResetBoardLoadHelper : IBoardLoadHelper
    {
        IDailyResetService _resetService = null!;

        public int Order => 0;
        public Type GetKey() { return GetType(); }

        public async Task UpdateOnBoardLoad(WebContext context)
        {
            await _resetService.DailyReset(context);
        }
    }
}
