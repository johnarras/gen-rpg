using MessagePack;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.PlayMultiplier.Messages
{
    [MessagePackObject]
    public class SetPlayMultResult : IWebResult
    {
        [Key(0)] public bool Success { get; set; }
        [Key(1)] public long NewPlayMult { get; set; }
    }
}
