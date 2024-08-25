using Genrpg.RequestServer.BoardGame.Helpers.BoardLoadHelpers;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.BoardGame.Services
{
    public class BoardLoadService : IBoardLoadService
    {
        private SetupDictionaryContainer<Type, IBoardLoadHelper> _boardLoadHelpers = new SetupDictionaryContainer<Type, IBoardLoadHelper>();
        public async Task AfterBoardLoad(WebContext context)
        {
            await Task.CompletedTask;
        }
    }
}
