using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Accounts.PlayerData
{
    [DataCategory(Category = DataCategoryTypes.AccountData)]
    public abstract class BaseAccountData : IStringId
    {
        public abstract string Id { get; set; }
    }
}
