using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Units.Mappers
{
    public class OwnerIdDataMapper<TParent, TChild, TApi> : OwnerDataMapper<TParent, TChild, TApi>
        where TParent : OwnerObjectList<TChild>, new()
        where TChild : OwnerPlayerData, IChildUnitData, IId
        where TApi : OwnerApiList<TParent,TChild>
    {
    }
}
