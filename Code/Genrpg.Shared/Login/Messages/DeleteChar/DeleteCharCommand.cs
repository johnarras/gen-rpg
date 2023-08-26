using MessagePack;
using Genrpg.Shared.Login.Interfaces;

namespace Genrpg.Shared.Login.Messages.DeleteChar
{
    [MessagePackObject]
    public class DeleteCharCommand : ILoginCommand
    {
        [Key(0)] public string CharId { get; set; }
    }
}
