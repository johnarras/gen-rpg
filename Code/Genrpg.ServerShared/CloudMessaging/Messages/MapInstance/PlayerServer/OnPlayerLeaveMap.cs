using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudMessaging.Messages.MapInstance.PlayerServer
{
    public class OnPlayerLeaveMap : IMapInstanceCloudMessage
    {
        public string Id { get; set; }
    }
}
