using MessagePack;
using System;

namespace Genrpg.Shared.DataStores.DataGroups
{
    [MessagePackObject]
    public class DataGroup : Attribute
    {
        [IgnoreMember] public EDataCategories Category { get; set; }
        [IgnoreMember] public ERepoTypes RepoType { get; set; }

        public DataGroup(EDataCategories category, ERepoTypes repoType)
        {
            Category = category;
            RepoType = repoType;
        }
    }

}
