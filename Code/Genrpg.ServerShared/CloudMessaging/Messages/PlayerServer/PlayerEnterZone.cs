using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudMessaging.Messages.PlayerServer
{
    public class PlayerEnterZone : IPlayerCloudMessage
    {
        public string Id { get; set; }
        public long ZoneId { get; set; }
    }
}
