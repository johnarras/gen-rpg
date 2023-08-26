using MessagePack;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Categories
{
    [DataCategory(Category = DataCategory.Content)]
    [MessagePackObject]
    public class BaseContentData : IStringId
    {
        [Key(0)] public string Id { get; set; }
    }
}
