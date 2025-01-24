using MessagePack;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.PlayMultiplier.WebApi
{
    [MessagePackObject]
    public class SetPlayMultRequest : IClientUserRequest
    {
        [Key(0)] public long PlayMult { get; set; }
    }
}
