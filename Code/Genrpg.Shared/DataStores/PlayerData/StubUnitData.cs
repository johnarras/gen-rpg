using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
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

        public void Delete(IRepositorySystem repoSystem) { }
        public void Save(IRepositorySystem repoSystem, bool saveClean) { }

        private bool _isDirty = false;
        public void SetDirty(bool val) { _isDirty = val; }
        public bool IsDirty() { return _isDirty; }
    }
}
