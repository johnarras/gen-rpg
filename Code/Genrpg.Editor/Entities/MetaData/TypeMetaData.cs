using Genrpg.Shared.DataStores.Categories.GameSettings;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Editor.Entities.MetaData
{
    [MessagePackObject]
    public class TypeMetaData : ChildSettings
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public override string Name { get; set; }
        [Key(3)] public string TypeName { get; set; }
        [Key(4)] public List<MemberMetaData> MemberData { get; set; } = new List<MemberMetaData>();

    }
}
