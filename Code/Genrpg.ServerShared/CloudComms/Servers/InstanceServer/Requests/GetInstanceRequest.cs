using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Requests
{
    public class GetInstanceRequest : IInstanceServerRequest
    {
        public string MapId { get; set; }
    }
}
