using MessagePack;
using Genrpg.Shared.Login.Interfaces;

namespace Genrpg.Shared.Login.Messages.CreateChar
{
    [MessagePackObject]
    public class CreateCharCommand : ILoginCommand
    {
        [Key(0)] public string Name { get; set; }
    }
}
