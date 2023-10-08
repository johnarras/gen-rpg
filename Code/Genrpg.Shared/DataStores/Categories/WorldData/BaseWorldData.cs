using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Categories.WorldData
{
    [DataCategory(Category = DataCategory.WorldData)]
    public abstract class BaseWorldData : IStringId
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }
        public abstract void Delete(IRepositorySystem repoSystem);
    }
}
