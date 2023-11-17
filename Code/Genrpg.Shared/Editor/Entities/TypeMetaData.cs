using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.GameSettings;
using System.Collections.Generic;

namespace Genrpg.Shared.Editor.Entities
{
    [MessagePackObject]
    public class TypeMetaData : ChildSettings
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public string TypeName { get; set; }
        [Key(3)] public List<MemberMetaData> MemberData { get; set; } = new List<MemberMetaData>();

    }
}
