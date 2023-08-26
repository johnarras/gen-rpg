using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudMessaging.Messages.InstanceServer
{
    public class RemoveMapServer : IInstanceCloudMessage
    {
        public string ServerId { get; set; }
    }
}
