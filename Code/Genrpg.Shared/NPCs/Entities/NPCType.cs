using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.WorldData;

namespace Genrpg.Shared.NPCs.Entities
{
    /// <summary>
    /// Use this class for extended monsters.
    /// </summary>
    [MessagePackObject]
    public class NPCType : BaseWorldData, IIndexedGameItem, IStringOwnerId
    {
        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string OwnerId { get; set; }
        
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public string MapId { get; set; }
        [Key(4)] public long ZoneId { get; set; }
        [Key(5)] public string Name { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Desc { get; set; }
        [Key(8)] public string Art { get; set; }

        [Key(9)] public long UnitTypeId { get; set; }

        [Key(10)] public long FactionTypeId { get; set; }

        [Key(11)] public long Level { get; set; }
        [Key(12)] public long QualityTypeId { get; set; }

        [Key(13)] public int MapX { get; set; }
        [Key(14)] public int MapZ { get; set; }

        [Key(15)] public int ItemCount { get; set; }
        [Key(16)] public long ItemQualityTypeId { get; set; }



        public NPCType()
        {
        }

    }
}
