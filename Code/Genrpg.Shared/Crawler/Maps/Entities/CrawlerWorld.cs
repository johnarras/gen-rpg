using MessagePack;
using Genrpg.Shared.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Maps.Entities
{
    [MessagePackObject]
    public class CrawlerWorld : IStringId, IIdName
    {
        [Key(0)] public string Id { get; set; }
        [Key(1)] public long IdKey { get; set; }
        [Key(2)] public string Name { get; set; }

        [JsonIgnore]
        [Key(3)] public List<CrawlerMap> Maps { get; set; } = new List<CrawlerMap>();

        [Key(4)] public List<WorldQuestItem> QuestItems { get; set; } = new List<WorldQuestItem>();

        [Key(5)] public long MaxMapId { get; set; } 


        public CrawlerMap CreateMap(CrawlerMapGenData genData, int width, int height)
        {
            long mapId = ++MaxMapId;
            CrawlerMap map = new CrawlerMap()
            {
                Id = "Map" + mapId,
                CrawlerMapTypeId = genData.MapType,
                Looping = genData.Looping,
                Width = width,
                Height = height,
                Level = genData.Level,
                IdKey = mapId,
                MapFloor = genData.CurrFloor,
            };

            map.SetupDataBlocks();
            Maps.Add(map);

            return map;
        }

        public CrawlerMap GetMap(long mapId)
        {
            return Maps.FirstOrDefault(x => x.IdKey == mapId);
        }

    }
}
