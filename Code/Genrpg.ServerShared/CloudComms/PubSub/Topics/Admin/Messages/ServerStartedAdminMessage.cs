using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages
{
    public class ServerStartedAdminMessage : BaseAdminPubSubMessage
    {
        public string ServerId { get; set; }
    }
}
