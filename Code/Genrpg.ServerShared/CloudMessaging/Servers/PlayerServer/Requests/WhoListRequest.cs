using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudMessaging.Servers.PlayerServer.Requests
{
    public class WhoListRequest : IPlayerCloudRequest
    {
        public string Args { get; set; }
    }
}
