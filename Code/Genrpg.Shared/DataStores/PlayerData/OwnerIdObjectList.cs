using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.DataStores.PlayerData
{
    public abstract class OwnerIdObjectList<TChild> : OwnerObjectList<TChild> where TChild : OwnerPlayerData, IId, new()
    {
        protected object _dataLock = new object();


        protected ConcurrentDictionary<long, TChild> _lookup = new ConcurrentDictionary<long, TChild>();

        virtual protected bool CreateMissingChildOnGet() { return true; }

        virtual protected void OnCreateChild(TChild newChild)
        {

        }
        public override void SetData(List<TChild> data)
        {
            base.SetData(data);
            _lookup.Clear();
            foreach (TChild child in data)
            {
                _lookup[child.IdKey] = child;
            }
        }

        public TChild Get(long id)
        {
            if (_lookup.TryGetValue(id, out TChild child))
            {
                return child;
            }

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
                        _lookup[child.IdKey] = child;
                    }
                }
            }
            return child;
        }
    }
}
