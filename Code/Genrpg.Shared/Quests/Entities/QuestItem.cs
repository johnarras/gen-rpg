using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Entities;

namespace Genrpg.Shared.Quests.Entities
{
    [MessagePackObject]
    public class QuestItem : BaseWorldData, IIndexedGameItem, IStringOwnerId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string OwnerId { get; set; }

        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }


        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }
    }
}
