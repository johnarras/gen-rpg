using MessagePack;
using System.Collections.Generic;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Website.Messages.Signup
{
    [MessagePackObject]
    public class SignupCommand : IAuthCommand
    {
        [Key(0)] public string Email { get; set; }
        [Key(1)] public string Password { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public string ShareId { get; set; }
        [Key(4)] public string ReferrerId { get; set; }
        [Key(5)] public long AccountProductId { get; set; }
    }
}
