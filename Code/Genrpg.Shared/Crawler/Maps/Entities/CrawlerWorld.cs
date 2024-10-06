using Genrpg.Shared.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Maps.Entities
{
    public class CrawlerWorld : IStringId, IIdName
    {
        public string Id { get; set; }
        public long IdKey { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public List<CrawlerMap> Maps { get; set; } = new List<CrawlerMap>();

        public List<WorldQuestItem> QuestItems { get; set; } = new List<WorldQuestItem>();

        public long MaxMapId { get; set; } = 0;

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
                ZoneTypeId = genData.ZoneTypeId,
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
