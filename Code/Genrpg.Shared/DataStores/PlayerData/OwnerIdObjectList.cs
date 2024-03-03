using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.DataStores.PlayerData
{
    public abstract class OwnerIdObjectList<Child> : OwnerObjectList<Child> where Child : OwnerPlayerData, IId, new()
    {
        protected List<Child> _data { get; set; } = new List<Child>();
        protected object _dataLock = new object();

        virtual protected bool CreateMissingChildOnGet() { return true; }

        public override void SetData(List<Child> data)
        {
            _data = data;
        }

        public override List<Child> GetData()
        {
            return _data;
        }

        public Child Get(long id)
        {
            Child child = _data.FirstOrDefault(x => x.IdKey == id);

            if (child == null && CreateMissingChildOnGet())
            {
                lock (_dataLock)
                {
                    child = _data.FirstOrDefault(x => x.IdKey == id);
                    if (child == null)
                    {
                        child = new Child()
                        {
                            Id = HashUtils.NewGuid(),
                            OwnerId = Id,
                            IdKey = id,
                        };
                        _data = new List<Child>(_data) { child };
                    }
                }
            }
            return child;
        }
    }
}
