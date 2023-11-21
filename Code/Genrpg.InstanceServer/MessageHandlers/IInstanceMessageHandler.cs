using Genrpg.ServerShared.CloudComms.Queues.Entities;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.InstanceServer.MessageHandlers
{
    public interface IInstanceMessageHandler : IQueueMessageHandler
    {
    }
}
