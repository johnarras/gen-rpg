using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.DataStores.PlayerData
{
    public abstract class OwnerObjectList<TChild> : BasePlayerData, IUnitData where TChild : OwnerPlayerData
    {
        public abstract void SetData(List<TChild> data);
        public abstract List<TChild> GetData();

        public override void Save(IRepositorySystem repoSystem, bool saveClean)
        {
            base.Save(repoSystem, saveClean);

            foreach (TChild child in GetData())
            {
                child.Save(repoSystem, saveClean);
            }
        }
        public override void Delete(IRepositorySystem repoSystem)
        {
            base.Delete(repoSystem);
            foreach (TChild child in GetData())
            {
                child.Delete(repoSystem);
            }
        }
    }
}
