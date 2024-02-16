using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.MainServer
{
    public class InitMapServerData
    {
        public int MapServerCount { get; set; }
        public int MapServerIndex { get; set; }
        public string MapServerId { get; set; }
        public List<string> MapIds { get; set; }
        public int StartPort { get; set; }

        public InitMapServerData()
        {
            MapIds = new List<string>();
        }

    }
}
