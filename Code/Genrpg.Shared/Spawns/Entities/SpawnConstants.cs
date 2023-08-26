using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spawns.Entities
{
    [MessagePackObject]
    public class SpawnConstants
    {
        public const int DefaultSpawnSeconds = 30;
    }
}
