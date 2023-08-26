using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Spawns.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.MapServer.Maps.Messaging
{
    public sealed class RespawnObject : BaseMapMessage
    {
        public IMapSpawn Spawn { get; set; }
    }
}