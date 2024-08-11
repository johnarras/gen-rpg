using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Interfaces;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Resets.Services
{
    public class ResetService : IResetService
    {

        protected IServiceLocator _loc;

        private SetupDictionaryContainer<Type, IResetHelper> _resetHelpers = new SetupDictionaryContainer<Type, IResetHelper>();
        //private List<IResetHelper> _helpers = null;

        public async Task DailyReset(WebContext context)
        {
            await Task.CompletedTask;
        }
    }
}
