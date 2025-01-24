using MessagePack;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Sexes.Constants;

namespace Genrpg.Shared.Characters.WebApi.CreateChar
{
    [MessagePackObject]
    public class CreateCharRequest : IClientUserRequest
    {
        [Key(0)] public string Name { get; set; }
        [Key(1)] public long UnitTypeId { get; set; } = 1;
        [Key(2)] public long SexTypeId { get; set; } = SexTypes.Male;
    }
}
