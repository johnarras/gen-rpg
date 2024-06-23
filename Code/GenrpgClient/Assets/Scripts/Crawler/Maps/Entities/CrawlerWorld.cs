using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.UI.Crawler.States;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Maps.Entities
{
    public class CrawlerWorld : IStringId, IIdName
    {
        public string Id { get; set; }
        public long IdKey { get; set; }
        public string Name { get; set; }




        public List<CrawlerMap> Maps { get; set; } = new List<CrawlerMap>();

        public long GetMaxMapId()
        {
            if (Maps.Count < 1)
            {
                return 0;
            }
            return Maps.Max(x => x.IdKey);
        }

        private long GetNextMapId()
        {
           return GetMaxMapId() + 1;
        }

        public CrawlerMap CreateMap(ECrawlerMapTypes mapType, bool looping, int width, int height)
        {
            CrawlerMap map = new CrawlerMap()
            {
                MapType = mapType,
                Looping = looping,
                Width = width,
                Height = height,
                IdKey = GetNextMapId(),
            };
            Maps.Add(map);
            return map;
        }

        public CrawlerMap GetMap(long mapId)
        {
            return Maps.FirstOrDefault(x=>x.IdKey == mapId);
        }

    }
}
