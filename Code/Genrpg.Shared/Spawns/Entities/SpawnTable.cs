using MessagePack;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
namespace Genrpg.Shared.Spawns.Entities
{
    [MessagePackObject]
    public class SpawnTable : IIndexedGameItem
    {

        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string NameId { get; set; }
        [Key(3)] public string Desc { get; set; }
        [Key(4)] public string Icon { get; set; }
        [Key(5)] public List<SpawnItem> Items { get; set; }
        [Key(6)] public string Art { get; set; }

        public SpawnTable()
        {
            Items = new List<SpawnItem>();
        }
    }
}
