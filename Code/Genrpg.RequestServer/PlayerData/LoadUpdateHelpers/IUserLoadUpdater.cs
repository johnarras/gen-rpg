using Genrpg.RequestServer.Core;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.PlayerData.LoadUpdateHelpers
{
    public interface IUserLoadUpdater : IOrderedSetupDictionaryItem<Type>
    {
        int Order { get; }
        Task Update(WebContext context, List<IUnitData> unitData);
    }
}
