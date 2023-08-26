using Genrpg.Shared.Networking.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Maps
{
    public class InitMapInstanceData
    {
        public string MapId { get; set; }
        public string MapServerId { get; set; }
        public string InstanceId { get; set; }
        public string MapServerMessageQueueId { get; set; }
        public int Port { get; set; }
        public EMapApiSerializers Serializer { get; set; } = EMapApiSerializers.MessagePack;
    }
}
