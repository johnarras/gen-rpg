using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Units.Mappers
{
    public class OwnerDataMapper<TParent, TChild, TApi> : UnitDataMapper<TParent>
        where TParent : OwnerObjectList<TChild>, new()
        where TChild : OwnerPlayerData, IChildUnitData
        where TApi : OwnerApiList<TParent, TChild>
    {
        public override IUnitData MapToAPI(IUnitData serverObject)
        {
            TParent parent = serverObject as TParent;

            TApi api = Activator.CreateInstance<TApi>();

            api.ParentObj = parent;
            api.Data = parent.GetData().ToList();

            return api;
        }
    }
}
