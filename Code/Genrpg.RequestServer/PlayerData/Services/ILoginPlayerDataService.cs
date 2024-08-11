using Genrpg.RequestServer.Core;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.PlayerData.Services
{
    public interface ILoginPlayerDataService : IInitializable
    {
        Task<List<IUnitData>> LoadPlayerDataOnLogin(WebContext context, Character ch = null);
    }
}
