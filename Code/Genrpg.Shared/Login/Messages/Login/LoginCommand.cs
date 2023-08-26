using MessagePack;
using Genrpg.Shared.Login.Interfaces;

namespace Genrpg.Shared.Login.Messages.Login
{
    [MessagePackObject]
    public class LoginCommand : ILoginCommand
    {
        [Key(0)] public string Email { get; set; }
        [Key(1)] public string Password { get; set; }
        [Key(2)] public string Name { get; set; }
    }
}
