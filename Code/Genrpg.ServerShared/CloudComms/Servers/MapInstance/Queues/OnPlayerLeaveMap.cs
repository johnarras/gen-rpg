﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Servers.MapInstance.Queues
{
    public class OnPlayerLeaveMap : IMapInstanceQueueMessage
    {
        public string Id { get; set; }
    }
}
