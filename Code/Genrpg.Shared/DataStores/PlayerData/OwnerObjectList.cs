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
        protected List<TChild> _data = new List<TChild>();
        public virtual void SetData(List<TChild> data)
        {
            _data = data;
        }

        public virtual IReadOnlyList<TChild> GetData()
        {
            return _data;
        }

        public override void QueueSave(IRepositoryService repoService)
        {
            base.QueueSave(repoService);

            foreach (TChild child in GetData())
            {
                child.QueueSave(repoService);
            }
        }

        public override void QueueDelete(IRepositoryService repoService)
        {
            base.QueueDelete(repoService);
            foreach (TChild child in GetData())
            {
                child.QueueDelete(repoService);
            }
        }

    }
}
