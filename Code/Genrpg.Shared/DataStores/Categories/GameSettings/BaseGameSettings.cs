using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.DataStores.Categories.GameSettings
{
    [DataCategory(Category = DataCategoryTypes.GameData)]
    public abstract class BaseGameSettings : IGameSettings, IUpdateData
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }
        [MessagePack.IgnoreMember]
        public virtual string Name
        {
            get { return GetType().Name; }
            set { }
        }
        [MessagePack.IgnoreMember]
        public DateTime UpdateTime { get; set; } = DateTime.MinValue;
        [MessagePack.IgnoreMember]
        public DateTime CreateTime { get; set; } = DateTime.MinValue;

        public abstract void AddTo(GameData data);

        public virtual void SetInternalIds() { }
        public virtual void ClearIndex() { }

        public virtual async Task SaveAll(IRepositoryService repo)
        {
            await repo.Save(this);
        }
        public virtual List<IGameSettings> GetChildren() { return new List<IGameSettings>(); }

    }
}
