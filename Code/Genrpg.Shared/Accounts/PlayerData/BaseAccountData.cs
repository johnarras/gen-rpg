using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Accounts.PlayerData
{
    [DataGroup(EDataCategories.Accounts,ERepoTypes.NoSQL)]
    public abstract class BaseAccountData : IStringId
    {
        public abstract string Id { get; set; }
    }
}
