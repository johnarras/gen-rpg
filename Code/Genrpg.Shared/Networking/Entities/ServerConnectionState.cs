using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Networking.Interfaces;
using Genrpg.Shared.Characters.PlayerData;

namespace Genrpg.Shared.Networking.Entities
{
    [MessagePackObject]
    public class ServerConnectionState
    {
        [Key(0)] public IConnection conn { get; set; }
        [Key(1)] public Character ch { get; set; }
    }
}
