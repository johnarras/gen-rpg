using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues
{
    public class AddMapInstance : IInstanceQueueMessage
    {
        public string MapFullServerId { get; set; }
        public string MapId { get; set; }
        public string InstanceId { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
