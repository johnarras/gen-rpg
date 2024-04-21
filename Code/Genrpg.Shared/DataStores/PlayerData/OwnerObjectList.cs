using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.DataStores.PlayerData
{
    public abstract class OwnerObjectList<TChild> : BasePlayerData, ITopLevelUnitData where TChild : OwnerPlayerData
    {
        public abstract void SetData(List<TChild> data);
        public abstract List<TChild> GetData();

        public override void Save(IRepositoryService repoSystem, bool saveClean)
        {
            base.Save(repoSystem, saveClean);

            foreach (TChild child in GetData())
            {
                child.Save(repoSystem, saveClean);
            }
        }

        public override List<BasePlayerData> GetSaveObjects(bool saveClean)
        {
            List<BasePlayerData> retval = new List<BasePlayerData>();

            retval.AddRange(base.GetSaveObjects(saveClean));

            foreach (TChild child in GetData())
            {
                retval.AddRange(child.GetSaveObjects(saveClean));
            }

            return retval;
        }


        public override void Delete(IRepositoryService repoSystem)
        {
            base.Delete(repoSystem);
            foreach (TChild child in GetData())
            {
                child.Delete(repoSystem);
            }
        }

    }
}
