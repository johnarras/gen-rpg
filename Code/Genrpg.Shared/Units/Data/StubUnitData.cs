using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Units.Data
{
    public abstract class StubUnitData : IUnitData
    {
        public string Id { get; set; }
        public abstract void AddTo(Unit unit);
        public abstract void SaveAll(IRepositorySystem repoSystem);
        public abstract void Delete(IRepositorySystem repoSystem);

        private bool _isDirty = false;
        public void SetDirty(bool val) { _isDirty = val; }
        public bool IsDirty() { return _isDirty; }
    }
}
