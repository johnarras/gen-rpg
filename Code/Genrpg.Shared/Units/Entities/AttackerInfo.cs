using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Units.Entities
{
    [MessagePackObject]
    public class AttackerInfo
    {
        [Key(0)] public string AttackerId { get; set; }
        [Key(1)] public string GroupId { get; set; }
    }
}
