using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.WorldData;

namespace Genrpg.Shared.Quests.WorldData
{
    [MessagePackObject]
    public class QuestItem : BaseWorldData, IIndexedGameItem, IMapOwnerId
    {
        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string OwnerId { get; set; }
        [Key(2)] public string MapId { get; set; }
        [Key(3)] public long IdKey { get; set; }
        [Key(4)] public string Name { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }


    }
}
