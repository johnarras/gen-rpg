using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.DataStores.Categories.WorldData
{
    [DataGroup(EDataCategories.Worlds,ERepoTypes.NoSQL)]
    public abstract class BaseWorldData : IStringId
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }
        public abstract void Delete(IRepositoryService repoSystem);
    }
}
