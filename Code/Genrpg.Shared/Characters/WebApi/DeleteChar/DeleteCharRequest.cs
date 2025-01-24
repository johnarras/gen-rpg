using MessagePack;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Characters.WebApi.DeleteChar
{
    [MessagePackObject]
    public class DeleteCharRequest : IClientUserRequest
    {
        [Key(0)] public string CharId { get; set; }
    }
}
