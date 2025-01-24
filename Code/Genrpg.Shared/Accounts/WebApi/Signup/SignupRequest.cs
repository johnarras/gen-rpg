using MessagePack;
using System.Collections.Generic;
using Genrpg.Shared.Website.Interfaces;
using System;

namespace Genrpg.Shared.Accounts.WebApi.Signup
{
    [MessagePackObject]
    public class SignupRequest : IAuthLoginRequest
    {
        [Key(0)] public string Email { get; set; }
        [Key(1)] public string Password { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public string ShareId { get; set; }
        [Key(4)] public string ReferrerId { get; set; }
        [Key(5)] public long AccountProductId { get; set; }
        [Key(6)] public string ClientVersion { get; set; }

        [Key(7)] public string DeviceId { get; set; }
    }
}
