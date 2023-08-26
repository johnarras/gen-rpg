using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.DataStores.Indexes
{
    [MessagePackObject]
    public class IndexConfig
    {
        [Key(0)] public string MemberName { get; set; }
        [Key(1)] public bool Ascending { get; set; } = true;
    }
}
