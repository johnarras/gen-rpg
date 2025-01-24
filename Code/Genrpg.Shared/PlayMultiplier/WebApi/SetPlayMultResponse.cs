using MessagePack;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.PlayMultiplier.WebApi
{
    [MessagePackObject]
    public class SetPlayMultResponse : IWebResponse
    {
        [Key(0)] public bool Success { get; set; }
        [Key(1)] public long NewPlayMult { get; set; }
    }
}
