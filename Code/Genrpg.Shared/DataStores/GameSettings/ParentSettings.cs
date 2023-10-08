using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.GameSettings
{
    public abstract class ParentSettings<TChild> : BaseGameSettings
        where TChild : ChildSettings, new()
    {
        public abstract List<TChild> Data { get; set; }
        public virtual void SetData(List<TChild> data) { Data = data; }
        public List<TChild> GetData() { return Data; }
        public override void SetInternalIds()
        {
            for (int c = 0; c < Data.Count; c++)
            {
                TChild child = Data[c];

                string oldParentId = child.ParentId;
                child.ParentId = Id;
                if (string.IsNullOrEmpty(child.Id))
                {
                    string childId = child.GetType().Name;
                    if (child is IId iid)
                    {
                        childId += iid.IdKey;
                    }
                    else
                    {
                        childId += HashUtils.NewGuid();
                    }
                    childId += Id;
                    childId = childId.ToLower();
                    child.Id = childId;
                }
                if (!string.IsNullOrEmpty(oldParentId))
                {
                    child.Id = child.Id.Replace(oldParentId, "");
                }
                child.Id += Id;
            }
        }
    }
}
