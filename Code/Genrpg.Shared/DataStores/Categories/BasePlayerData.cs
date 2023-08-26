using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Categories
{
    [DataCategory(Category = DataCategory.PlayerData)]
    public abstract class BasePlayerData : IStringId
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }
        public abstract void Delete(IRepositorySystem repoSystem);

        public BasePlayerData()
        {

        }
    }
}
