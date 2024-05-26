using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.DataStores.PlayerData
{
    public abstract class OwnerIdObjectList<TChild> : OwnerObjectList<TChild> where TChild : OwnerPlayerData, IId, new()
    {
        protected object _dataLock = new object();

        virtual protected bool CreateMissingChildOnGet() { return true; }

        virtual protected void OnCreateChild(TChild newChild)
        {

        }

        public TChild Get(long id)
        {
            TChild child = _data.FirstOrDefault(x => x.IdKey == id);

            if (child == null && CreateMissingChildOnGet())
            {
                lock (_dataLock)
                {
                    child = _data.FirstOrDefault(x => x.IdKey == id);
                    if (child == null)
                    {
                        child = new TChild()
                        {
                            Id = HashUtils.NewGuid(),
                            OwnerId = Id,
                            IdKey = id,
                        };
                        OnCreateChild(child);
                        _data = new List<TChild>(_data) { child };
                    }
                }
            }
            return child;
        }
    }
}
