using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues
{
    public class LogoutUser : IPlayerQueueMessage
    {
        public string Id { get; set; }
    }
}
