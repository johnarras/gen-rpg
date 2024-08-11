using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.DataGroups;

namespace Genrpg.Shared.DataStores.Categories.ContentData
{
    [DataGroup(EDataCategories.Settings,ERepoTypes.Blob)]
    [MessagePackObject]
    public abstract class BaseGameContentData : IStringId
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }
    }
}
