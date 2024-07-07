using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Maps.Entities
{
    public class WorldQuestItem : IIdName
    {
        public long IdKey { get; set; }
        public string Name { get; set; }
        public long FoundInMapId { get; set; }
        public long UnlocksMapId { get; set; }
    }
}
