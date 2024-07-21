using MessagePack;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Website.Messages.DeleteChar
{
    [MessagePackObject]
    public class DeleteCharResult : IWebResult
    {
        [Key(0)] public List<CharacterStub> AllCharacters { get; set; }
    }
}
