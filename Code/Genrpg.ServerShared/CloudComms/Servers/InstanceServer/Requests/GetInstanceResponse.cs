using Genrpg.ServerShared.CloudComms.Requests.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Requests
{
    public class GetInstanceResponse : IResponse
    {
       public string InstanceId { get; set; }
       public string Host { get; set; }
       public long Port { get; set; }
    }
}
