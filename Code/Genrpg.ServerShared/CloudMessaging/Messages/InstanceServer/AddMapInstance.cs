using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudMessaging.Messages.InstanceServer
{
    public class AddMapInstance : IInstanceCloudMessage
    {
        public string MapInstanceId { get; set; }
        public string MapId { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
