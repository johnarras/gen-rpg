using MessagePack;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Website.Messages.DeleteChar
{
    [MessagePackObject]
    public class DeleteCharCommand : IClientCommand
    {
        [Key(0)] public string CharId { get; set; }
    }
}
