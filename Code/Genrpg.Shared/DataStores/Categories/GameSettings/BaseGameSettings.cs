﻿using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.DataStores.Categories.GameSettings
{
    [DataGroup(EDataCategories.Settings,ERepoTypes.NoSQL)]
    public abstract class BaseGameSettings : IGameSettings
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
