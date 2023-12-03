using MessagePack;
using Genrpg.Shared.Login.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Login.Messages.Login
{
    [MessagePackObject]
    public class LoginCommand : ILoginCommand
    {
        [Key(0)] public string Email { get; set; }
        [Key(1)] public string Password { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public List<ClientCachedGameSettings> ClientSettings { get; set; } = new List<ClientCachedGameSettings>();
    }
}
