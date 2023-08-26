using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudMessaging.Messages.PlayerServer
{
    public class LogoutUser : IPlayerCloudMessage
    {
        public string Id { get; set; }
    }
}
