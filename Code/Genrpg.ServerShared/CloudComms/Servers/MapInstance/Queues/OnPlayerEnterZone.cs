﻿using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Servers.MapInstance.Queues
{
    public class OnPlayerEnterZone : IMapInstanceQueueMessage
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long ZoneId { get; set; }
    }
}
