using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.DataGroups;

namespace Genrpg.Shared.DataStores.Categories.ContentData
{
    [DataGroup(EDataCategories.Accounts,ERepoTypes.Blob)]
    [MessagePackObject]
    public abstract class BaseAccountContentData : IStringId
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }
    }
}
