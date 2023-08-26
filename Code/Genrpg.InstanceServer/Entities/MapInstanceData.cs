using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.InstanceServer.Entities
{
    public class MapInstanceData
    {
        public string MapId { get; set; }
        public string InstanceId { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
