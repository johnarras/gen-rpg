using MessagePack;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.PlayMultiplier.Messages
{
    [MessagePackObject]
    public class SetPlayMultCommand : IClientCommand
    {
        [Key(0)] public long PlayMult { get; set; }
    }
}
