using MessagePack;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Characters.PlayerData
{

    [MessagePackObject]
    public class CharacterStub : IStringId
    {
        [Key(0)] public string Id { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public long Level { get; set; }
    }
}
