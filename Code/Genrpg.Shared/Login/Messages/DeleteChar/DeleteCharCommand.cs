using MessagePack;
using Genrpg.Shared.Login.Interfaces;

namespace Genrpg.Shared.Login.Messages.DeleteChar
{
    [MessagePackObject]
    public class DeleteCharCommand : IClientCommand
    {
        [Key(0)] public string CharId { get; set; }
    }
}
