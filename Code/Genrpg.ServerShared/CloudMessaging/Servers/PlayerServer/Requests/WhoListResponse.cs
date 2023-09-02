using Genrpg.ServerShared.CloudMessaging.Requests;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudMessaging.Servers.PlayerServer.Requests
{

    public class WhoListChar
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long Level { get; set; }
        public string ZoneName { get; set; }
    }

    public class WhoListResponse : ICloudResponse
    {
        public List<WhoListChar> Chars { get; set; } = new List<WhoListChar>();
    }
}
