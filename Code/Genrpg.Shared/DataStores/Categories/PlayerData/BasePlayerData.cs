using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Players.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Genrpg.Shared.DataStores.Categories.PlayerData
{
    [DataCategory(Category = DataCategory.PlayerData)]
    public abstract class BasePlayerData : IUnitData
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }

        public virtual void AddTo(Unit unit) { unit.Set(this); }

        public virtual void Save(IRepositorySystem repoSystem, bool saveClean)
        {
            if (saveClean || IsDirty())
            {
                repoSystem.QueueSave(this);
                SetDirty(false);
            }
        }

        public virtual List<BasePlayerData> GetSaveObjects(bool saveClean)
        {
            List<BasePlayerData> retval = new List<BasePlayerData>();
            if (saveClean || IsDirty())
            {
                SetDirty(false);
                retval.Add(this);
            }
            return retval;
        }

        public virtual void Delete(IRepositorySystem repoSystem)
        {
            repoSystem.QueueDelete(this);
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
