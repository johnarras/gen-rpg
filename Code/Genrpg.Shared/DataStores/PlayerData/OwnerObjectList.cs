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
            _data.ForEach(x => x.SetRepo(_repoService));
        }

        public override void SetRepo(IRepositoryService repoService)
        {
            base.SetRepo(repoService);
            _data.ForEach(x=>x.SetRepo(_repoService));
        }

        public virtual IReadOnlyList<TChild> GetData()
        {
            return _data;
        }

        public override void Save()
        {
            base.Save();

            foreach (TChild child in GetData())
            {
                child.Save();
            }
        }

        public override void Delete()
        {
            base.Delete();
            foreach (TChild child in GetData())
            {
                child.Delete();
            }
        }

    }
}
