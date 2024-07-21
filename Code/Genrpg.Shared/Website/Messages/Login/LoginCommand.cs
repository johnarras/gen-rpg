using MessagePack;
using System.Collections.Generic;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Website.Messages.Login
{
    [MessagePackObject]
    public class LoginCommand : IAuthCommand
    {
        [Key(0)] public List<ClientCachedGameSettings> ClientSettings { get; set; } = new List<ClientCachedGameSettings>();
        [Key(1)] public string UserId { get; set; }
        [Key(2)] public string Email { get; set; }
        [Key(3)] public string Password { get; set; }
        
        [Key(4)] public long AccountProductId { get; set; }

        [Key(5)] public string ReferrerId { get; set; }
    }
}
