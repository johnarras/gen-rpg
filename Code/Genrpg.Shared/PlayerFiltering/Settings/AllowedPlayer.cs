using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.PlayerFiltering.Settings
{
    [MessagePackObject]
    public class AllowedPlayer
    {
        [Key(0)] public string PlayerId { get; set; }
    }
}
