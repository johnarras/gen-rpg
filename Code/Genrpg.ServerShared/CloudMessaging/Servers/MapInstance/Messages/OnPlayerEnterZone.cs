using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudMessaging.Servers.MapInstance.Messages
{
    public class OnPlayerEnterZone : IMapInstanceCloudMessage
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long ZoneId { get; set; }
    }
}
