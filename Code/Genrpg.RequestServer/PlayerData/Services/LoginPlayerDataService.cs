using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.PlayerData.LoadUpdateHelpers;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.PlayerData.Services
{
    public class LoginPlayerDataService : ILoginPlayerDataService
    {

        OrderedSetupDictionaryContainer<Type, ICharacterLoadUpdater> _characterLoadUpdateHelpers = new OrderedSetupDictionaryContainer<Type, ICharacterLoadUpdater>();
        OrderedSetupDictionaryContainer<Type, IUserLoadUpdater> _userLoadUpdateHelpers = new OrderedSetupDictionaryContainer<Type, IUserLoadUpdater>();
        private IPlayerDataService _playerDataService = null!;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public async Task<List<IUnitData>> LoadPlayerDataOnLogin(WebContext context, Character ch = null)
        {
            List<IUnitData> dataList = await _playerDataService.LoadAllPlayerData(context.rand, context.user, ch);

            if (ch != null)
            {
                foreach (IUnitData data in dataList)
                {
                    data.AddTo(ch);
                }

                await UpdateCharacterOnLoad(context, ch);
            }
            else
            {
                await UpdateUserOnLoad(context, dataList);
            }
            return dataList;
        }

        protected async Task UpdateCharacterOnLoad(WebContext context, Character ch)
        {
            foreach (ICharacterLoadUpdater updater in _characterLoadUpdateHelpers.OrderedItems())
            {
                await updater.Update(context, ch);
            }
        }

        protected async Task UpdateUserOnLoad(WebContext context, List<IUnitData> userUnitData)
        {
            foreach (IUserLoadUpdater updater in _userLoadUpdateHelpers.OrderedItems())
            {
                await updater.Update(context, userUnitData);
            }        
        }
    }
}
