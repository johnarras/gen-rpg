using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Core
{
    public abstract class OwnerObjectList<Child> : BasePlayerData, IUnitData where Child : OwnerPlayerData
    {
        public abstract void AddTo(Unit unit);
        public abstract void SetData(List<Child> data);
        public abstract List<Child> GetData();

        public virtual void SaveAll(IRepositorySystem repoSystem)
        {
            foreach (Child child in GetData())
            {
                repoSystem.QueueSave(child);
            }
        }

        public override void Delete(IRepositorySystem repoSystem)
        { 
            repoSystem.Delete(this);
            foreach (Child child in GetData())
            {
                repoSystem.QueueDelete(child);
            }
        }

        protected bool _isDirty = false;
        public bool IsDirty() { return _isDirty; }
        public void SetDirty(bool dirty) { _isDirty = dirty; }
    }
}
