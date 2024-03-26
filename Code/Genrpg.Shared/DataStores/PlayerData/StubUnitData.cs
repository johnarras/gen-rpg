using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.PlayerData
{
    public abstract class StubUnitData : IUnitData
    {
        public string Id { get; set; }
        public virtual void AddTo(Unit unit) { unit.Set(this); }

        public void Delete(IRepositoryService repoSystem) { }
        public void Save(IRepositoryService repoSystem, bool saveClean) { }

        private bool _isDirty = false;
        public void SetDirty(bool val) { _isDirty = val; }
        public bool IsDirty() { return _isDirty; }
        public virtual List<IUnitData> GetChildren() { return new List<IUnitData>(); }

        public List<BasePlayerData> GetSaveObjects(bool saveClean)
        {
            return new List<BasePlayerData>();
        }
    }
}
