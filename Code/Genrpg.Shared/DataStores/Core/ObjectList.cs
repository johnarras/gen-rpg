using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Core
{
    public abstract class ObjectList<T> : BasePlayerData, IUnitData
    {
        [MessagePack.IgnoreMember]
        public abstract List<T> Data { get; set; }

        public abstract void AddTo(Unit unit);
        public virtual void SaveAll(IRepositorySystem repoSystem) { }

        public ObjectList()
        {
            Data = new List<T>();
        }

        private bool _isDirty = false;
        public void SetDirty(bool val)
        {
            _isDirty = val;
        }

        public bool IsDirty()
        {
            return _isDirty;
        }
    }
}
