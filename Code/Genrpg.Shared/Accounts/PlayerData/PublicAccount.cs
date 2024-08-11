using Genrpg.Shared.DataStores.Categories.ContentData;
using Genrpg.Shared.DataStores.Constants;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Accounts.PlayerData
{
    [MessagePackObject]
    public class PublicAccount : BaseAccountContentData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string Name { get; set; }

    }
}
