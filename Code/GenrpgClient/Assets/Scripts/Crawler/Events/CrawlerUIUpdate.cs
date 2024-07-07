using Assets.Scripts.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Events
{
    public class CrawlerUIUpdate
    {
        public CrawlerWorld World { get; set; }
        public CrawlerMap Map { get; set; }        
        public PartyData Party { get; set; }
    }
}
