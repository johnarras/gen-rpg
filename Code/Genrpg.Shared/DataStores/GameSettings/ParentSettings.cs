using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.DataStores.GameSettings
{
    public abstract class ParentSettings<TChild> : BaseGameSettings, IComplexCopy
        where TChild : ChildSettings, new()
    {
        protected List<TChild> _data { get; set; } = new List<TChild>();
        public virtual void SetData(List<TChild> data) { _data = data; ClearIndex(); }
        public List<TChild> GetData() { return _data; }
        public override void SetInternalIds()
        {

            for (int c = 0; c < _data.Count; c++)
            {
                TChild child = _data[c];

                string oldParentId = child.ParentId;
                child.ParentId = Id;

                string childId = child.Id;
                if (child is IId iid)
                {
                    childId = child.GetType().Name + iid.IdKey + Id;
                }
                else if (string.IsNullOrEmpty(childId))
                {
                    childId = child.GetType().Name + HashUtils.NewGuid().ToString();
                }
                childId = childId.ToLower();
                child.Id = childId;

                if (!string.IsNullOrEmpty(oldParentId))
                {
                    child.Id = child.Id.Replace(oldParentId, "");
                }
                child.Id += Id;
            }
        }


        public override async Task SaveAll(IRepositorySystem repo)
        {
            await repo.Save(this);
            await repo.SaveAll(_data);
        }


        public override List<IGameSettings> GetChildren() { return new List<IGameSettings>(_data); }

        public void DeepCopyFrom(IComplexCopy from)
        {
            if (from.GetType() == GetType())
            {             
                Id = "copy" + (DateTime.UtcNow.Ticks % 1000000);
                List<TChild> fromChildren = from.GetDeepCopyData() as List<TChild>;
                if (fromChildren != null)
                {
                    List<TChild> newData = new List<TChild>();
                    foreach (TChild fromChild in fromChildren)
                    {
                        newData.Add(SerializationUtils.FastMakeCopy(fromChild));
                    }
                    SetData(newData);
                    SetInternalIds();
                }
            }
        }

        public object GetDeepCopyData()
        {
            return _data;
        }
    }
}
