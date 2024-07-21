using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Indexes
{
    [MessagePackObject]
    public class CreateIndexData
    {
        [Key(0)] public List<IndexConfig> Configs { get; set; } = new List<IndexConfig>();
    }
}
