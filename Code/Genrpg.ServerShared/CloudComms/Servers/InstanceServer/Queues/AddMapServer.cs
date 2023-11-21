using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues
{
    public class AddMapServer : IInstanceQueueMessage
    {
        public string ServerId { get; set; }
    }
}
