using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Units.Loaders
{
    public class OwnerIdDataLoader<TParent, TChild> : OwnerDataLoader<TParent,TChild>
        where TParent : OwnerObjectList<TChild>, new()
        where TChild : OwnerPlayerData, IChildUnitData, IId
    {
        public override async Task<IChildUnitData> LoadChildByIdkey (Unit unit, long idkey)
        {

            TParent parentObj = (TParent)await LoadTopLevelData(unit);

            TChild child = parentObj.GetData().FirstOrDefault(x => x.IdKey == idkey);

            if (child != null)
            {
                List<TChild> currList = parentObj.GetData().ToList();
                currList.Add(child);
                parentObj.SetData(currList);
            }

            return child;
        }
    }
}
