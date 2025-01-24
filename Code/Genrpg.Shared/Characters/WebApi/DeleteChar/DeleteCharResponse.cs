using MessagePack;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Characters.WebApi.DeleteChar
{
    [MessagePackObject]
    public class DeleteCharResponse : IWebResponse
    {
        [Key(0)] public List<CharacterStub> AllCharacters { get; set; }
    }
}
