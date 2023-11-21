using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues
{
    public class RemoveMapInstance : IInstanceQueueMessage
    {
        public string FullInstanceId { get; set; }
    }
}
